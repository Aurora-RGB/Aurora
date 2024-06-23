﻿using System.Drawing;
using System.Windows.Controls;
using AuroraRgb.EffectsEngine;
using AuroraRgb.Profiles.Minecraft.GSI;
using AuroraRgb.Settings;
using AuroraRgb.Settings.Layers;
using AuroraRgb.Utils;

namespace AuroraRgb.Profiles.Minecraft.Layers {

    public class MinecraftBackgroundLayerHandlerProperties : LayerHandlerProperties2Color<MinecraftBackgroundLayerHandlerProperties> {

        public MinecraftBackgroundLayerHandlerProperties() : base() { }
        public MinecraftBackgroundLayerHandlerProperties(bool assign_default = false) : base(assign_default) { }

        public override void Default() {
            base.Default();

            _PrimaryColor = Color.FromArgb(200, 255, 240);
            _SecondaryColor = Color.FromArgb(30, 50, 60);
            _Sequence = new KeySequence(new FreeFormObject(0, -60, 900, 300));
        }
    }

    public class MinecraftBackgroundLayerHandler : LayerHandler<MinecraftBackgroundLayerHandlerProperties> {
        public MinecraftBackgroundLayerHandler() : base("Background Layer")
        {
        }

        protected override UserControl CreateControl() {
            return new Control_MinecraftBackgroundLayer(this);
        }

        public override EffectLayer Render(IGameState gamestate) {
            if (gamestate is GameStateMinecraft) {

                long time = (gamestate as GameStateMinecraft).World.WorldTime;

                if (time >= 1000 && time <= 11000) // Between 1000 and 11000, world is fully bright day time
                    EffectLayer.Set(Properties.Sequence, Properties.PrimaryColor);
                else if (time <= 14000) // Between 11000 and 14000 world transitions from day to night
                    EffectLayer.Set(Properties.Sequence, ColorUtils.BlendColors(Properties.PrimaryColor, Properties.SecondaryColor, ((float)(time - 11000) / 3000)));
                else if (time <= 22000) // Between 14000 and 22000 world is fully night time
                    EffectLayer.Set(Properties.Sequence, Properties.SecondaryColor);
                else // Between 22000 and 1000 world is transitions from night to day
                    EffectLayer.Set(Properties.Sequence, ColorUtils.BlendColors(Properties.SecondaryColor, Properties.PrimaryColor, (((float)(time + 2000) % 24000) / 3000))); // This weird calculation converts range (22,1) into range (0,1) respecting that 24000 = 0
            }
            return EffectLayer;
        }
    }
}
