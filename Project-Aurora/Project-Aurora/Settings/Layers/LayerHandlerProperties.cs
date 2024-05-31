using System;
using System.ComponentModel;
using System.Drawing;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;
using AuroraRgb.Profiles;
using AuroraRgb.Settings.Overrides;
using Common.Utils;
using FastMember;
using JetBrains.Annotations;
using Newtonsoft.Json;

namespace AuroraRgb.Settings.Layers;

[UsedImplicitly(ImplicitUseKindFlags.InstantiatedWithFixedConstructorSignature, ImplicitUseTargetFlags.WithInheritors)]
public abstract class LayerHandlerProperties<TProperty> : IValueOverridable, INotifyPropertyChanged, IDisposable
    where TProperty : LayerHandlerProperties<TProperty>
{
    private static readonly Lazy<TypeAccessor> Accessor = new(() => TypeAccessor.Create(typeof(TProperty), false));

    public event PropertyChangedEventHandler? PropertyChanged;

    [GameStateIgnore, JsonIgnore] public TProperty? Logic { get; private set; }

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

    [JsonIgnore] public Color PrimaryColor => Logic?._PrimaryColor ?? _PrimaryColor ?? Color.Empty;

    [JsonIgnore] private KeySequence _sequence;

    [LogicOverridable("Affected Keys")]
    public virtual KeySequence _Sequence
    {
        get => _sequence;
        set => SetFieldAndRaisePropertyChanged(out _sequence, value);
    }

    [JsonIgnore] public KeySequence Sequence => Logic?._Sequence ?? _Sequence;


    #region Override Special Properties

    // These properties are special in that they are designed only for use with the overrides system and
    // allows the overrides to access properties not actually present on the Layer.Handler.Properties.
    // Note that this is NOT the variable that is changed when the user changes one of these settings in the
    // UI (for example not changed when an item in the layer list is enabled/disabled with the checkbox).
    [LogicOverridable("Enabled")] public bool? _Enabled { get; set; }
    [JsonIgnore] public bool Enabled => Logic?._Enabled ?? _Enabled ?? true;

    // Renamed to "Layer Opacity" so that if the layer properties needs an opacity for whatever reason, it's
    // less likely to have a name collision.
    [LogicOverridable("Opacity")] public float? _LayerOpacity { get; set; }
    [JsonIgnore] public float LayerOpacity => Logic?._LayerOpacity ?? _LayerOpacity ?? 1f;

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

        Logic = (TProperty)Activator.CreateInstance(typeof(TProperty), new object[] { true })!;
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
        try
        {
            if (Accessor.Value[Logic, propertyName] == value)
            {
                return;
            }

            if (value != null && value.Equals(Accessor.Value[Logic, propertyName])) return;
            if (value == null)
            {
                try
                {
                    Accessor.Value[Logic, propertyName] = value;
                }
                catch (NullReferenceException)
                {
                    //handle primitive type unboxing
                }

                return;
            }

            Accessor.Value[Logic, propertyName] = value;
        }
        catch (ArgumentOutOfRangeException)
        {
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

public class LayerHandlerProperties : LayerHandlerProperties<LayerHandlerProperties>
{
    public LayerHandlerProperties()
    {
    }

    public LayerHandlerProperties(bool assignDefault = false) : base(assignDefault)
    {
    }
}