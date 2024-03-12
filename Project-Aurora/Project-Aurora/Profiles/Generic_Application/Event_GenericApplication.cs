﻿using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using AuroraRgb.EffectsEngine;
using AuroraRgb.Profiles.Generic;
using AuroraRgb.Settings.Layers;
using AuroraRgb.Utils;

namespace AuroraRgb.Profiles.Generic_Application
{
    public class Event_GenericApplication : GameEvent_Generic
    {
        public Event_GenericApplication()
        {
        }

        public override void UpdateLights(EffectFrame frame)
        {
            Queue<EffectLayer> layers = new Queue<EffectLayer>();

            GenericApplicationProfile settings = (GenericApplicationProfile)this.Application.Profile;

            ObservableCollection<Layer> timeLayers = settings.Layers;

            //Scripts
            //this.Application.UpdateEffectScripts(layers);

            if ((Global.Configuration.NighttimeEnabled &&
                Time.IsCurrentTimeBetween(Global.Configuration.NighttimeStartHour, Global.Configuration.NighttimeStartMinute, Global.Configuration.NighttimeEndHour, Global.Configuration.NighttimeEndMinute)) ||
                settings._simulateNighttime
                )
            {
                timeLayers = settings.Layers_NightTime;
            }

            foreach (var layer in timeLayers.Reverse().ToArray())
            {
                if (layer.Enabled)
                    layers.Enqueue(layer.Render(_game_state));
            }

            frame.AddLayers(layers.ToArray());
        }
    }
}
