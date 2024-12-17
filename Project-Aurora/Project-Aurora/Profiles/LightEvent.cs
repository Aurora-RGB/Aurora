using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AuroraRgb.EffectsEngine;

namespace AuroraRgb.Profiles;

public interface ILightEvent : IDisposable, IAsyncDisposable
{
    void UpdateLights(EffectFrame frame);

    void UpdateOverlayLights(EffectFrame newFrame);

    void SetGameState(IGameState newGameState);

    void ResetGameState(Type? gameStateType = null);

    void OnStart();

    void OnStop();

    bool IsEnabled { get; }
    bool IsOverlayEnabled { get; }

    LightEventConfig Config { get; }

    Task<bool> Initialize(CancellationToken cancellationToken);
}

/// <summary>
/// Class responsible for applying EffectLayers to an EffectFrame based on GameState information.
/// </summary>
public class LightEvent : ILightEvent
{
    public Application Application { get; set; }
    public LightEventConfig Config { get; }

    public IGameState GameState { get; protected set; } = new NewtonsoftGameState();

    public LightEvent()
    {

    }

    public LightEvent(LightEventConfig config) : this()
    {
        Config = config;
    }

    /// <summary>
    /// Adds new layers to the passed EffectFrame instance based on GameState information.
    /// </summary>
    /// <param name="frame">EffectFrame instance to which layers will be added</param>
    public virtual void UpdateLights(EffectFrame frame) {
        UpdateTick();

        var layers = Application.Profile.Layers
            .Where(l => l.Enabled)
            .Reverse()
            .Select(l => l.Render(GameState));
        frame.AddLayers(layers);
    }

    /// <summary>
    /// Adds new layers to the overlay of the passed EffectFrame.
    /// </summary>
    public virtual void UpdateOverlayLights(EffectFrame frame) {
        try
        {
            var overlayLayers = Application.Profile.OverlayLayers
                .Where(l => l.Enabled)
                .Reverse()
                .Select(l => l.Render(GameState));
            frame.AddOverlayLayers(overlayLayers);
        }
        catch(Exception e)
        {
            Global.logger.Error(e, "Error updating overlay layers");
        }
    }

    /// <summary>
    /// This method is called during the default implementation of UpdateLights. Appliation-specific updates that do not need
    /// to edit layers or the frame (which will be the vast majority of them) should go in here and use the default UpdateLights.
    /// If more control over layers/frame is needed, this method should be ignored and UpdateLights should be overriden instead.
    /// </summary>
    public virtual void UpdateTick() { }

    /// <summary>
    /// Adds new layers to the passed EffectFrame instance based on GameState information as well as process a new GameState instance.
    /// </summary>
    /// <param name="newGameState">GameState instance which will be processed before adding new layers</param>
    public virtual void SetGameState(IGameState newGameState)
    {
        GameState = newGameState;
    }

    /// <summary>
    /// Returns whether or not this LightEvent is active
    /// </summary>
    /// <returns>A boolean value representing if this LightEvent is active</returns>
    public virtual bool IsEnabled => Application?.Settings?.IsEnabled ?? true;

    public virtual bool IsOverlayEnabled => Application?.Settings?.IsOverlayEnabled ?? true;

    public bool Initialized { get; private set; }

    public virtual void ResetGameState(Type? gameStateType = null)
    {
        if (gameStateType != null)
            GameState = (IGameState)Activator.CreateInstance(gameStateType);
        else if (Application?.Config?.GameStateType != null)
            GameState = (IGameState)Activator.CreateInstance(Application.Config.GameStateType);
        else
            GameState = new NewtonsoftGameState();
    }
        
    public virtual void OnStart()
    {

    }

    public virtual void OnStop()
    {

    }

    public Task<bool> Initialize(CancellationToken cancellationToken)
    {
        if (Initialized)
            return Task.FromResult(true);

        Initialized = Init();

        return Task.FromResult(Initialized);
    }

    protected virtual bool Init()
    {
        return true;
    }

    public virtual void Dispose()
    {

    }

    public virtual ValueTask DisposeAsync()
    {
        return ValueTask.CompletedTask;
    }
}