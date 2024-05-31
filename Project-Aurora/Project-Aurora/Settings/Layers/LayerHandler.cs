using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
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
public abstract class LayerHandler<TProperty> : ILayerHandler where TProperty : LayerHandlerProperties<TProperty>
{
    [JsonIgnore]
    public Application Application { get; protected set; }

    [JsonIgnore]
    private Task<UserControl>? _Control;

    [JsonIgnore]
    public Task<UserControl> Control => _Control ??= CreateControlOnMain();

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

    object ILayerHandler.Properties {
        get => Properties;
        set => Properties = value as TProperty;
    }
        
    public bool EnableSmoothing { get; set; }

    // Always return true if the user is overriding the exclusion zone (so that we don't have to present the user with another
    // option in the overrides asking if they want to enabled/disable it), otherwise if there isn't an overriden value for
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

    public float Opacity => Properties.LayerOpacity;
    public float? _Opacity {
        get => Properties._LayerOpacity;
        set => Properties._LayerOpacity = value;
    }

    [JsonIgnore]
    private TextureBrush? _previousRender; //Previous layer

    [JsonIgnore]
    private TextureBrush? _previousSecondRender; //Layer before previous

    [JsonIgnore]
    private readonly Temporary<EffectLayer> _effectLayer;

    private static PropertyChangedEventArgs ConstPropertyChangedEventArgs = new("");
    protected EffectLayer EffectLayer
    {
        get
        {
            if (!_effectLayer.HasValue)
            {
                var _ = _effectLayer.Value;
                PropertiesChanged(this, ConstPropertyChangedEventArgs);
            }
            return _effectLayer.Value;
        }
    }

    protected LayerHandler(): this("Unoptimized Layer"){}

    protected LayerHandler(string name)
    {
        var colorMatrix1 = new ColorMatrix
        {
            Matrix33 = 0.6f
        };
        _prevImageAttributes = new();
        _prevImageAttributes.SetColorMatrix(colorMatrix1);
        var colorMatrix2 = new ColorMatrix
        {
            Matrix33 = 0.4f
        };
        _secondPrevImageAttributes = new();
        _secondPrevImageAttributes.SetColorMatrix(colorMatrix2);

        _effectLayer = new(() => new EffectLayer(name, true));
        _ExclusionMask = new KeySequence();
        Properties.PropertyChanged += PropertiesChanged;
        WeakEventManager<Effects, EventArgs>.AddHandler(null, nameof(Effects.CanvasChanged), PropertiesChanged);
    }

    public virtual EffectLayer Render(IGameState gameState)
    {
        return EffectLayer.EmptyLayer;
    }

    public virtual void SetGameState(IGameState gamestate)
    {

    }

    private readonly Lazy<EffectLayer> _postfxLayer = new(() => new EffectLayer("PostFXLayer", true));
    private readonly ImageAttributes _prevImageAttributes;
    private readonly ImageAttributes _secondPrevImageAttributes;

    public EffectLayer PostRenderFX(EffectLayer renderedLayer)
    {
        if (EnableSmoothing)
        {
            SmoothLayer(renderedLayer);
        }

        //Last PostFX is exclusion
        renderedLayer.Exclude(EnableExclusionMask ? ExclusionMask : KeySequence.Empty);

        renderedLayer *= Properties.LayerOpacity;

        return renderedLayer;
    }

    private void SmoothLayer(EffectLayer renderedLayer)
    {
        var returnLayer = _postfxLayer.Value;
        returnLayer.Clear();

        var g = returnLayer.GetGraphics();
        {
            g.CompositingMode = CompositingMode.SourceOver;
            g.CompositingQuality = CompositingQuality.HighSpeed;
            g.SmoothingMode = SmoothingMode.None;
            g.InterpolationMode = InterpolationMode.Low;

            g.DrawImage(renderedLayer.TextureBrush.Image,
                renderedLayer.Dimension, renderedLayer.Dimension,
                GraphicsUnit.Pixel
            );
            if (_previousRender != null)
            {
                g.FillRectangle(_previousRender, renderedLayer.Dimension);
            }
            if (_previousSecondRender != null)
            {
                g.FillRectangle(_previousSecondRender, renderedLayer.Dimension);
            }
        }

        try
        {
            if (_previousRender != null)
            {
                _previousSecondRender = new TextureBrush(_previousRender.Image, renderedLayer.Dimension, _secondPrevImageAttributes);
            }
            //Update previous layers
            _previousRender = new TextureBrush(renderedLayer.TextureBrush.Image, renderedLayer.Dimension, _prevImageAttributes);
        }
        catch (Exception e) //canvas changes
        {
            _previousSecondRender = EffectLayer.EmptyLayer.TextureBrush;
            _previousRender = EffectLayer.EmptyLayer.TextureBrush;
        }
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
        PropertiesChanged(this, new PropertyChangedEventArgs(string.Empty));
    }
        
    protected virtual void PropertiesChanged(object? sender, PropertyChangedEventArgs args)
    {
            
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
public abstract class LayerHandler(string name) : LayerHandler<LayerHandlerProperties>(name);