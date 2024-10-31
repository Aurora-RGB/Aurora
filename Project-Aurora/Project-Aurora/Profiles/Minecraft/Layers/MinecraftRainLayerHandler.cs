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

public class MinecraftRainLayerHandler() : LayerHandler<MinecraftRainLayerHandlerProperties, BitmapEffectLayer>("Minecraft Rain Layer")
{
    private readonly List<Droplet> _raindrops = new();
    private readonly Random _rnd = new();
    private int _frame;

    protected override UserControl CreateControl() {
        return new Control_MinecraftRainLayer(this);
    }

    private void CreateRainDrop() {
        var randomX = (float)_rnd.NextDouble() * Effects.Canvas.Width;
        _raindrops.Add(new Droplet(
            new AnimationMix([
                new AnimationTrack("raindrop", 0)
                    .SetFrame(0, new AnimationFilledRectangle(randomX, 0, 3, 6, Properties.PrimaryColor))
                    .SetFrame(1, new AnimationFilledRectangle(randomX + 5, Effects.Canvas.Height, 2, 4, Properties.PrimaryColor))
            ])
        ));
    }

    public override EffectLayer Render(IGameState gamestate) {
        if (gamestate is not GameStateMinecraft minecraft) return EmptyLayer.Instance;

        // Add more droplets based on the intensity
        float strength = minecraft.World.RainStrength;
        if (strength > 0) {
            if (_frame <= 0) {
                // calculate time (in frames) until next droplet is created
                float min = Properties.MinimumInterval, max = Properties.MaximumInterval; // Store as floats so C# doesn't prematurely round numbers
                _frame = (int)Math.Round(min - (min - max) * strength); // https://www.desmos.com/calculator/uak73e5eub
                CreateRainDrop();
            } else
                _frame--;
        }

        // Render all droplets
        var graphics = EffectLayer.GetGraphics();
        foreach (var droplet in _raindrops) {
            droplet.Mix.Draw(graphics, droplet.Time);
            droplet.Time += .1f;
        }

        // Remove any expired droplets
        _raindrops.RemoveAll(droplet => droplet.Time >= 1);

        return EffectLayer;
    }
}

internal class Droplet(AnimationMix mix)
{
    internal AnimationMix Mix = mix;
    internal float Time;
}