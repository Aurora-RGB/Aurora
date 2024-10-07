using System;
using System.ComponentModel;
using System.Drawing;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;
using AuroraRgb.Profiles;
using AuroraRgb.Settings.Layers.Exceptions;
using AuroraRgb.Settings.Overrides;
using Common.Utils;
using JetBrains.Annotations;
using Newtonsoft.Json;

namespace AuroraRgb.Settings.Layers;

[UsedImplicitly(ImplicitUseKindFlags.InstantiatedWithFixedConstructorSignature, ImplicitUseTargetFlags.WithInheritors)]
public class LayerHandlerProperties : IValueOverridable, INotifyPropertyChanged, IDisposable
{
    public event PropertyChangedEventHandler? PropertyChanged;

    [GameStateIgnore, JsonIgnore] 
    public LayerHandlerPropertiesLogic? Logic { get; set; }

    [JsonIgnore] private Color? _primaryColor;

    [LogicOverridable("Primary Color")]
    public Color? _PrimaryColor
    {
        get => _primaryColor;
        set
        {
            _primaryColor = value;
            PropertyChanged?.Invoke(null, new PropertyChangedEventArgs(""));
        }
    }

    [JsonIgnore] public Color PrimaryColor
    {
        get => Logic?._PrimaryColor ?? _PrimaryColor ?? Color.Empty;
        set => _primaryColor = value;
    }

    [JsonIgnore] private KeySequence _sequence;

    [LogicOverridable("Affected Keys")]
    public virtual KeySequence _Sequence
    {
        get => _sequence;
        set => SetFieldAndRaisePropertyChanged(out _sequence, value);
    }

    [JsonIgnore] public KeySequence Sequence
    {
        get => Logic?._Sequence ?? _Sequence;
        set => _Sequence = value;
    }

    #region Override Special Properties

    // These properties are special in that they are designed only for use with the overrides system and
    // allows the overrides to access properties not actually present on the Layer.Handler.Properties.
    // Note that this is NOT the variable that is changed when the user changes one of these settings in the
    // UI (for example not changed when an item in the layer list is enable/disabled with the checkbox).
    [LogicOverridable("Enabled")] public bool? _Enabled { get; set; }
    [JsonIgnore] public bool Enabled => Logic?._Enabled ?? _Enabled ?? true;

    // Renamed to "Layer Opacity" so that if the layer properties needs an opacity for whatever reason, it's
    // less likely to have a name collision.
    [LogicOverridable("Opacity")] public double? _LayerOpacity { get; set; }
    [JsonIgnore] public double LayerOpacity => Logic?._LayerOpacity ?? _LayerOpacity ?? 1f;

    [LogicOverridable("Excluded Keys")] public KeySequence _Exclusion { get; set; }
    [JsonIgnore] public KeySequence Exclusion => Logic?._Exclusion ?? _Exclusion ?? new KeySequence();

    #endregion

    public LayerHandlerProperties() : this(false)
    {
        Default();
    }

    public LayerHandlerProperties(bool empty)
    {
        if (!empty)
            Default();
    }

    public virtual void Default()
    {
        if (Logic != null)
        {
            Logic.PropertyChanged -= OnPropertiesChanged;
        }

        Logic = GeneratedLogics.LogicMap[GetType().Name]();
        Logic.PropertyChanged += OnPropertiesChanged;
        _PrimaryColor = CommonColorUtils.GenerateRandomColor();
        if (_Sequence != null)
        {
            _Sequence.Freeform.ValuesChanged -= OnFreeformChanged;
        }

        _Sequence = new KeySequence();
        _Sequence.Freeform.ValuesChanged += OnFreeformChanged;
    }

    public void SetOverride(string propertyName, object? value)
    {
        if (Logic == null)
        {
            return;
        }
        if (Logic.SetterMap.TryGetValue(propertyName, out var setter))
        {
            setter(Logic, value);
            return;
        }

        if (propertyName.StartsWith('_') && Logic.SetterMap.TryGetValue(propertyName[1..], out _))
        {
            throw new OverrideNameRefactoredException();
        }
    }

    [OnDeserialized]
    public void OnDeserialized(StreamingContext context)
    {
        OnPropertiesChanged(this, new PropertyChangedEventArgs(""));
    }

    protected void SetFieldAndRaisePropertyChanged<T>(out T field, T newValue, [CallerMemberName] string propertyName = null)
    {
        field = newValue;
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    public void OnPropertiesChanged(object? sender, PropertyChangedEventArgs args)
    {
        PropertyChanged?.Invoke(sender, args);
    }

    public void OnPropertiesChanged(object? sender)
    {
        PropertyChanged?.Invoke(sender, new PropertyChangedEventArgs(""));
    }

    public void Dispose()
    {
        _Sequence.Freeform.ValuesChanged -= OnFreeformChanged;
    }

    private void OnFreeformChanged(object? sender, FreeFormChangedEventArgs e)
    {
        OnPropertiesChanged(sender, new PropertyChangedEventArgs(nameof(_Sequence.Freeform)));
    }
}
