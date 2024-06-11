using System.Collections.Generic;
using System.Linq;
using AuroraRgb.EffectsEngine;
using AuroraRgb.Profiles.Generic;

namespace AuroraRgb.Profiles.Generic_Application;

public class Event_GenericApplication : GameEvent_Generic
{
    public override void UpdateLights(EffectFrame frame)
    {
        var layers = new Queue<EffectLayer>();
        var settings = (GenericApplicationProfile)Application.Profile;
        var timeLayers = settings.Layers;

        foreach (var layer in timeLayers.Reverse().ToArray())
        {
            if (layer.Enabled)
                layers.Enqueue(layer.Render(GameState));
        }

        frame.AddLayers(layers.ToArray());
    }
}