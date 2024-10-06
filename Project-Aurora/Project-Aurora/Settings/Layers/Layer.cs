using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Windows.Controls;
using AuroraRgb.Bitmaps;
using AuroraRgb.EffectsEngine;
using AuroraRgb.Profiles;
using AuroraRgb.Settings.Layers.Exceptions;
using AuroraRgb.Settings.Overrides.Logic;
using AuroraRgb.Utils;
using Newtonsoft.Json;
using PropertyChanged;

namespace AuroraRgb.Settings.Layers;

/// <summary>
/// A class representing a default settings layer
/// </summary>
public class Layer : INotifyPropertyChanged, ICloneable, IDisposable
{
    [DoNotNotify, JsonIgnore]
    public Application? AssociatedApplication { get; private set; }

    public string Name { get; set; } = "New Layer";

    [OnChangedMethod(nameof(OnHandlerChanged))]
    public ILayerHandler Handler { get; set; } = new DefaultLayerHandler();

    [JsonIgnore]
    public Task<UserControl> Control => Handler.Control;

    public bool Enabled { get; set; } = true;

    public Dictionary<string, IOverrideLogic>? OverrideLogic { get; set; }
    // private void OnOverrideLogicChanged() => // Make the logic collection changed event trigger a property change to ensure it gets saved?

    #region Constructors
    public Layer() { }

    public Layer(string name, ILayerHandler? handler = null) : this() {
        Name = name;
        Handler = handler ?? Handler;
    }

    private Layer(string name, ILayerHandler handler, Dictionary<string, IOverrideLogic> overrideLogic) : this(name, handler) {
        OverrideLogic = overrideLogic;
    }

    public Layer(string name, ILayerHandler handler, OverrideLogicBuilder builder) : this(name, handler, new Dictionary<string, IOverrideLogic>(builder.Create())) { }
    #endregion

    public event PropertyChangedEventHandler? PropertyChanged;
    
    [JsonIgnore]
    public bool Error { get; private set; }

    private int _renderErrors;

    private void OnHandlerChanged() {
        if (AssociatedApplication != null)
            Handler.SetApplication(AssociatedApplication);
    }

    public EffectLayer Render(IGameState gs)
    {
        if (OverrideLogic != null)
        {
            // For every property which has an override logic assigned
            foreach (var (key, overrideLogic) in OverrideLogic)
                // Set the value of the logic evaluation as the override for this property
            {
                var value = overrideLogic.Evaluate(gs);
                try
                {
                    if (overrideLogic.VarType is { IsEnum: true })
                    {
                        ((IValueOverridable)Handler.Properties).SetOverride(key,
                            value == null ? null : Enum.ToObject(overrideLogic.VarType, value));
                    }
                    else
                    {
                        ((IValueOverridable)Handler.Properties).SetOverride(key, value);
                    }
                }
                catch (OverrideNameRefactoredException)
                {
                    OverrideLogic.Remove(key);
                    OverrideLogic.Add(key[1..], overrideLogic);
                    break;
                }
            }
        }

        if (!((dynamic)Handler.Properties).Enabled)
            return EffectLayer.EmptyLayer;
        try
        {
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
                return EffectLayer.EmptyLayer;
            }
            Global.logger.Error(e, "Layer render error");
        }

        return EffectLayer.EmptyLayer;
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
}

/// <summary>
/// Interface for layers that fire an event when the layer is rendered.<para/>
/// To use, the layer handler should call <code>LayerRender?.Invoke(this, layer.GetBitmap());</code> at the end of their <see cref="Layer.Render(IGameState)"/> method.
/// </summary>
public interface INotifyRender {
    event EventHandler<IAuroraBitmap> LayerRender;
}