using System.Linq;
using AuroraRgb.EffectsEngine;

namespace AuroraRgb.Profiles.Desktop;

public class Event_Desktop : LightEvent
{
    public override void UpdateLights(EffectFrame frame)
    {
        var layers = Application.Profile.Layers.Where(l => l.Enabled).Reverse().Select(l => l.Render(GameState));
        frame.AddLayers(layers);
    }

    public override void SetGameState(IGameState newGameState)
    {

    }
}