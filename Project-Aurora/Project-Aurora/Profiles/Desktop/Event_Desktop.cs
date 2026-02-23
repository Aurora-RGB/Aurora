using AuroraRgb.EffectsEngine;

namespace AuroraRgb.Profiles.Desktop;

public sealed class Event_Desktop : LightEvent
{
    public override void UpdateLights(EffectFrame frame)
    {
        var appLayers = Application.Profile.Layers;
        // Iterate through the layers in reverse order to ensure that the topmost layers are rendered last
        for (var i = appLayers.Count - 1; i >= 0; i--)
        {
            var layer = appLayers[i];
            if (layer.Enabled)
                frame.AddLayer(layer.Render(GameState));
        }
    }

    public override void SetGameState(IGameState newGameState)
    {

    }
}