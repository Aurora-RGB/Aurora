using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Controls;
using AuroraRgb.EffectsEngine;
using AuroraRgb.EffectsEngine.Animations;
using AuroraRgb.Profiles.Minecraft.GSI;
using AuroraRgb.Settings.Layers;
using Newtonsoft.Json;

namespace AuroraRgb.Profiles.Minecraft.Layers;

[Obsolete("This layer is obselete and has been replaced by the Overrides system.")]
public partial class MinecraftRainLayerHandlerProperties : LayerHandlerProperties {

    [JsonIgnore]
    public int MinimumInterval => _MinimumInterval ?? 30;
    public int? _MinimumInterval { get; set; }

    [JsonIgnore]
    public int MaximumInterval => _MaximumInterval ?? 30;
    public int? _MaximumInterval { get; set; }

    public override void Default() {
        base.Default();
        _PrimaryColor = Color.Cyan;
        _MinimumInterval = 30;
        _MaximumInterval = 1;
    }
}

public class MinecraftRainLayerHandler : LayerHandler<MinecraftRainLayerHandlerProperties> {

    private List<Droplet> raindrops = new();
    private Random rnd = new();
    private int frame;

    public MinecraftRainLayerHandler() : base("Minecraft Rain Layer")
    {
    }

    protected override UserControl CreateControl() {
        return new Control_MinecraftRainLayer(this);
    }

    private void CreateRainDrop() {
        var randomX = (float)rnd.NextDouble() * Effects.Canvas.Width;
        raindrops.Add(new Droplet() {
            mix = new AnimationMix(new[] {
                new AnimationTrack("raindrop", 0)
                    .SetFrame(0, new AnimationFilledRectangle(randomX, 0, 3, 6, Properties.PrimaryColor))
                    .SetFrame(1, new AnimationFilledRectangle(randomX + 5, Effects.Canvas.Height, 2, 4, Properties.PrimaryColor))
            }),
            time = 0
        });
    }

    public override EffectLayer Render(IGameState gamestate) {
        if (!(gamestate is GameStateMinecraft)) return EffectLayer.EmptyLayer;

        // Add more droplets based on the intensity
        float strength = (gamestate as GameStateMinecraft).World.RainStrength;
        if (strength > 0) {
            if (frame <= 0) {
                // calculate time (in frames) until next droplet is created
                float min = Properties.MinimumInterval, max = Properties.MaximumInterval; // Store as floats so C# doesn't prematurely round numbers
                frame = (int)Math.Round(min - (min - max) * strength); // https://www.desmos.com/calculator/uak73e5eub
                CreateRainDrop();
            } else
                frame--;
        }

        // Render all droplets
        var graphics = EffectLayer.GetGraphics();
        foreach (var droplet in raindrops) {
            droplet.mix.Draw(graphics, droplet.time);
            droplet.time += .1f;
        }

        // Remove any expired droplets
        raindrops.RemoveAll(droplet => droplet.time >= 1);

        return EffectLayer;
    }
}

internal class Droplet {
    internal AnimationMix mix;
    internal float time;
}