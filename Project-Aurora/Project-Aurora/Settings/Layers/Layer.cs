using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Controls;
using AuroraRgb.Bitmaps.GdiPlus;
using AuroraRgb.EffectsEngine;
using AuroraRgb.Profiles;
using AuroraRgb.Settings.Layers.Exceptions;
using AuroraRgb.Settings.Overrides;
using AuroraRgb.Settings.Overrides.Logic;
using AuroraRgb.Utils.Json;
using Newtonsoft.Json;
using PropertyChanged;

namespace AuroraRgb.Settings.Layers;

/// <summary>
/// A class representing a default settings layer
/// </summary>
public partial class Layer : INotifyPropertyChanged, ICloneable, IDisposable
{
    [DoNotNotify, JsonIgnore]
    public Application? AssociatedApplication { get; private set; }

    public string Name { get; set; } = "New Layer";

    [OnChangedMethod(nameof(OnHandlerChanged))]
    public ILayerHandler Handler { get; set; } = new DefaultLayerHandler();

    [JsonIgnore]
    public Task<UserControl> Control => Handler.Control;

    public bool Enabled { get; set; } = true;

    public Dictionary<string, IOverrideLogic> OverrideLogic { get; set; } = [];
    // private void OnOverrideLogicChanged() => // Make the logic collection changed event trigger a property change to ensure it gets saved?

    internal List<LayerPropertyViewModel> CachedPropertyList { get; private set; }

    #region Constructors

    public Layer()
    {
        CachedPropertyList = CreateOverridablePropertiesInternal();
    }

    public Layer(string name, ILayerHandler? handler = null) : this() {
        Name = name;
        Handler = handler ?? Handler;
    }

    private Layer(string name, ILayerHandler handler, Dictionary<string, IOverrideLogic> overrideLogic) : this(name, handler) {
        OverrideLogic = overrideLogic;
    }

    public Layer(string name, ILayerHandler handler, OverrideLogicBuilder builder) : this(name, handler, new Dictionary<string, IOverrideLogic>(builder.Create())) { }
    #endregion
    
    [JsonIgnore]
    public bool Error { get; private set; }

    private int _renderErrors;

    private List<LayerPropertyViewModel> CreateOverridablePropertiesInternal()
    {
        // Get a list of any members that should be ignored as per the LogicOverrideIgnorePropertyAttribute on the properties class
        var ignoredProperties = GetType().GetCustomAttributes(typeof(LogicOverrideIgnorePropertyAttribute), false)
            .Cast<LogicOverrideIgnorePropertyAttribute>()
            .Select(attr => attr.PropertyName);

        return Handler.Properties.GetType()
            .GetProperties() // Get all properties on the layer handler's property list
            .Where(prop => prop.GetCustomAttributes(typeof(LogicOverridableAttribute), true).Length > 0) // Filter to only return the PropertyInfos that have Overridable
            .Where(prop => !ignoredProperties.Contains(prop.Name)) // Only select things that are NOT on the ignored properties list
            .Select(prop => new LayerPropertyViewModel(prop, this))
            .OrderBy(tup => tup.DisplayName)
            .ToList();
    }

    private static readonly Dictionary<Type, Action<Layer, IGameState, string, IOverrideLogic>> OverrideTypeFuncs = new()
    {
        {
            typeof(bool), (layer, gs, key, overrideLogic) =>
            {
                var logicBoolVal = overrideLogic.EvaluateBool(gs, out var overridden);
                if (!overridden)
                {
                    layer.Handler.Properties.SetOverride(key, null);
                    return;
                }

                layer.Handler.Properties.SetOverride(key, logicBoolVal);
            }
        },
        {
            typeof(double), (layer, gs, key, overrideLogic) =>
            {
                var logicDoubleVal = overrideLogic.EvaluateDouble(gs, out var overridden);
                if (!overridden)
                {
                    layer.Handler.Properties.SetOverride(key, null);
                    return;
                }

                layer.Handler.Properties.SetOverride(key, logicDoubleVal);
            }
        },
        {
            typeof(Rectangle), (layer, gs, key, overrideLogic) =>
            {
                var logicRectangleVal = overrideLogic.EvaluateRectangle(gs, out var overridden);
                if (!overridden)
                {
                    layer.Handler.Properties.SetOverride(key, null);
                    return;
                }

                layer.Handler.Properties.SetOverride(key, logicRectangleVal);
            }
        },
        {
            typeof(Color), (layer, gs, key, overrideLogic) =>
            {
                var logicColorVal = overrideLogic.EvaluateColor(gs, out var overridden);
                if (!overridden)
                {
                    layer.Handler.Properties.SetOverride(key, null);
                    return;
                }

                layer.Handler.Properties.SetOverride(key, logicColorVal);
            }
        }
    };

    private void OnHandlerChanged() {
        if (AssociatedApplication != null)
            Handler.SetApplication(AssociatedApplication);
        CachedPropertyList = CreateOverridablePropertiesInternal();
    }

    public EffectLayer Render(IGameState gs)
    {
        try
        {
            // first check Enabled override, if it exists, and if it says disabled, return an empty layer
            if (OverrideLogic.TryGetValue(nameof(LayerHandlerProperties.Enabled), out var enabledLogic) ||
                OverrideLogic.TryGetValue(nameof(LayerHandlerProperties._Enabled), out enabledLogic))
            {
                var logicEnabled = enabledLogic.EvaluateBool(gs, out var overridden);
                if (overridden && !logicEnabled)
                    return EmptyLayer.Instance;
            }

            // For every property which has an override logic assigned
            foreach (var (key, overrideLogic) in OverrideLogic)
            {
                try
                {
                    if (OverrideTypeFuncs.TryGetValue(overrideLogic.VarType, out var overrideFunc))
                    {
                        overrideFunc(this, gs, key, overrideLogic);
                        continue;
                    }

                    // !!! THIS PATH GENERATES BOXING OVERHEAD !!!
                    // non-object values will generate lots of garbage memory allocations
                    var value = overrideLogic.Evaluate(gs);
                    switch (overrideLogic.VarType)
                    {
                        case { IsEnum: true }:
                            Handler.Properties.SetOverride(key,
                                value == null ? null : Enum.ToObject(overrideLogic.VarType, value));
                            break;
                        default:
                            Handler.Properties.SetOverride(key, value);
                            break;
                    }
                }
                catch (OverrideNameRefactoredException)
                {
                    OverrideLogic.Remove(key);
                    OverrideLogic.Add(key[1..], overrideLogic);
                    break;
                }
            }

            if (!Handler.Properties.Enabled)
                return EmptyLayer.Instance;
 
            var effectLayer = Handler.PostRenderFX(Handler.Render(gs));
            _renderErrors = 0;
            Error = false;
            return effectLayer;
        }
        catch (Exception e)
        {
            if (++_renderErrors == 3)
            {
                Error = true;
                var appAuroraApp = ((App)System.Windows.Application.Current).AuroraApp!;
                var controlInterface = appAuroraApp.ControlInterface;
                
                controlInterface.ShowErrorNotification($"Layer \'{Name}\" fails to render. Check logs for details");
                return EmptyLayer.Instance;
            }
            Global.logger.Error(e, "Layer render error");
        }

        return EmptyLayer.Instance;
    }

    public void SetProfile(Application profile) {
        AssociatedApplication = profile;
        Handler.SetApplication(AssociatedApplication);
    }

    public object Clone() {
        var str = JsonConvert.SerializeObject(this, Formatting.None, new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.Auto });
        return JsonConvert.DeserializeObject(
            str,
            GetType(),
            new JsonSerializerSettings
            {
                ObjectCreationHandling = ObjectCreationHandling.Replace,
                TypeNameHandling = TypeNameHandling.Auto,
                SerializationBinder = new AuroraSerializationBinder()
            }
        )!;
    }

    public void SetGameState(IGameState newGameState) => Handler.SetGameState(newGameState);
    public void Dispose() => Handler.Dispose();
    
    public void RemoveOverrideLogic(string propertyName)
    {
        OverrideLogic.Remove(propertyName);
        Handler.Properties.SetOverride(propertyName, null);

        // fire property changed
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(OverrideLogic)));
    }

    public void SetOverrideLogic(string propertyName, IOverrideLogic overrideLogic)
    {
        OverrideLogic[propertyName] = overrideLogic;

        // fire property changed
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(OverrideLogic)));
    }
}

/// <summary>
/// Interface for layers that fire an event when the layer is rendered.<para/>
/// To use, the layer handler should call <code>LayerRender?.Invoke(this, layer.GetBitmap());</code> at the end of their <see cref="Layer.Render(IGameState)"/> method.
/// </summary>
public interface INotifyRender {
    event EventHandler<GdiBitmap> LayerRender;
}