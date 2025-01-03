using System;
using System.ComponentModel;
using System.Runtime.Serialization;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using AuroraRgb.EffectsEngine;
using AuroraRgb.Profiles;
using AuroraRgb.Settings.Layers.Controls;
using AuroraRgb.Utils;
using JetBrains.Annotations;
using Newtonsoft.Json;
using Application = AuroraRgb.Profiles.Application;

namespace AuroraRgb.Settings.Layers;

[UsedImplicitly(ImplicitUseTargetFlags.WithInheritors)]
public abstract class LayerHandler<TProperty, LayerType> : ILayerHandler
    where TProperty : LayerHandlerProperties
    where LayerType : EffectLayer
{
    private readonly Temporary<Task<UserControl>> _control;

    [JsonIgnore]
    public Application Application { get; protected set; }

    [JsonIgnore]
    public Task<UserControl> Control => _control.Value;

    private TProperty _properties = Activator.CreateInstance<TProperty>();
    public TProperty Properties
    {
        get => _properties;
        set
        {
            _properties.Sequence.Freeform.ValuesChanged -= PropertiesChanged;
            _properties.PropertyChanged -= PropertiesChanged;
            _properties = value;
            value.PropertyChanged += PropertiesChanged;
            value.Sequence.Freeform.ValuesChanged += PropertiesChanged;
            value.OnPropertiesChanged(this);
        }
    }

    LayerHandlerProperties ILayerHandler.Properties {
        get => Properties;
        set => Properties = value as TProperty;
    }

    // Always return true if the user is overriding the exclusion zone (so that we don't have to present the user with another
    // option in the overrides asking if they want to enable/disable it), otherwise if there isn't an overriden value for
    // exclusion, simply return the value of the settings checkbox (as normal)
    [JsonIgnore]
    public bool EnableExclusionMask => Properties.Logic._Exclusion != null || (_EnableExclusionMask ?? false);
    public bool? _EnableExclusionMask { get; set; }

    [JsonIgnore]
    public KeySequence ExclusionMask => Properties.Exclusion;
    public KeySequence _ExclusionMask {
        get => Properties._Exclusion;
        set => Properties._Exclusion = value;
    }

    public double Opacity => Properties.LayerOpacity;
    public double? _Opacity {
        get => Properties._LayerOpacity;
        set => Properties._LayerOpacity = value;
    }

    [JsonIgnore]
    private readonly Temporary<EffectLayer> _effectLayer;

    private static readonly PropertyChangedEventArgs ConstPropertyChangedEventArgs = new("");
    protected LayerType EffectLayer
    {
        get
        {
            if (!_effectLayer.HasValue)
            {
                var _ = _effectLayer.Value;
                PropertiesChanged(this, ConstPropertyChangedEventArgs);
            }
            return (LayerType)_effectLayer.Value;
        }
    }

    protected bool Invalidated = true;

    private readonly string _name;

    protected LayerHandler(): this("Unoptimized Layer"){}

    protected LayerHandler(string name)
    {
        _effectLayer = new(() => CreateLayer(name));
        _ExclusionMask = new KeySequence();
        Properties.PropertyChanged += PropertiesChanged;
        WeakEventManager<Effects, EventArgs>.AddHandler(null, nameof(Effects.CanvasChanged), PropertiesChanged);

        _control = new Temporary<Task<UserControl>>(CreateControlOnMain, false);
        _name = name;
    }

    public override string ToString()
    {
        return _name;
    }

    private EffectLayer CreateLayer(string name)
    {
        if (typeof(LayerType) == typeof(BitmapEffectLayer))
        {
            return new BitmapEffectLayer(name, true);
        }
        if (typeof(LayerType) == typeof(NoRenderLayer))
        {
            return new NoRenderLayer();
        }

        throw new NotImplementedException();
    }

    public virtual EffectLayer Render(IGameState gameState)
    {
        return EmptyLayer.Instance;
    }

    public virtual void SetGameState(IGameState gamestate)
    {

    }

    public Type GetEffectLayerType()
    {
        return typeof(LayerType);
    }

    public virtual bool HighResource()
    {
        return false;
    }

    public EffectLayer PostRenderFX(EffectLayer renderedLayer)
    {
        //Last PostFX is exclusion
        renderedLayer.Exclude(EnableExclusionMask ? ExclusionMask : KeySequence.Empty);

        renderedLayer.SetOpacity(Properties.LayerOpacity);

        return renderedLayer;
    }

    public virtual async void SetApplication(Application profile)
    {
        Application = profile;
        await Initialize();
    }

    protected virtual Task Initialize()
    {
        return Task.CompletedTask;
    }

    private Task<UserControl> CreateControlOnMain()
    {
        var tcs = new TaskCompletionSource<UserControl>(TaskCreationOptions.RunContinuationsAsynchronously);
        System.Windows.Application.Current.Dispatcher.BeginInvoke(() =>
        {
            var layerControl = CreateControl();
            (layerControl as IProfileContainingControl)?.SetProfile(Application);
            tcs.SetResult(layerControl);
        }, DispatcherPriority.Loaded);
        return tcs.Task;
    }

    protected virtual UserControl CreateControl()
    {
        return new Control_DefaultLayer();
    }

    [OnDeserialized]
    [UsedImplicitly]
    private void OnDeserialized(StreamingContext context)
    {
        try
        {
            PropertiesChanged(this, new PropertyChangedEventArgs(string.Empty));
        }
        catch (Exception ex)
        {
            Global.logger.Error(ex, "Error deserializing layer");
        }
    }

    protected virtual void PropertiesChanged(object? sender, PropertyChangedEventArgs args)
    {
        Invalidated = true;
    }

    private void PropertiesChanged(object? sender, EventArgs e)
    {
        PropertiesChanged(sender, ConstPropertyChangedEventArgs);
    }

    private void PropertiesChanged(object? sender, FreeFormChangedEventArgs e)
    {
        PropertiesChanged(sender, ConstPropertyChangedEventArgs);
    }

    public virtual void Dispose()
    {
        if (_effectLayer.HasValue)
        {
            _effectLayer.Value.Dispose();
        }
        Properties.PropertyChanged -= PropertiesChanged;
        WeakEventManager<Effects, EventArgs>.RemoveHandler(null, nameof(Effects.CanvasChanged), PropertiesChanged);
    }
}

[LayerHandlerMeta(Exclude = true)]
public abstract class LayerHandler(string name) : LayerHandler<LayerHandlerProperties, NoRenderLayer>(name);

[LayerHandlerMeta(Exclude = true)]
public abstract class LayerHandler<Props>(string name) : LayerHandler<Props, NoRenderLayer>(name)
    where Props : LayerHandlerProperties;