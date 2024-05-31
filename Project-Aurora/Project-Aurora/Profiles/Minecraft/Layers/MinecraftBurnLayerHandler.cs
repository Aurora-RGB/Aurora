﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Controls;
using AuroraRgb.EffectsEngine;
using AuroraRgb.EffectsEngine.Animations;
using AuroraRgb.Profiles.Minecraft.GSI;
using AuroraRgb.Settings.Layers;

namespace AuroraRgb.Profiles.Minecraft.Layers;

[Obsolete("This layer is obselete and has been replaced by the Overrides system.")]
public class MinecraftBurnLayerHandler : LayerHandler<LayerHandlerProperties> {

    private List<FireParticle> particles = new();
    private Random rnd = new();

    public MinecraftBurnLayerHandler() : base("Minecraft Burning Layer")
    {
    }

    protected override UserControl CreateControl() {
        return new Control_MinecraftBurnLayer();
    }

    private void CreateFireParticle() {
        float randomX = (float)rnd.NextDouble() * Effects.Canvas.Width;
        float randomOffset = (float)rnd.NextDouble() * 15 - 7.5f;
        particles.Add(new FireParticle
        {
            mix = new AnimationMix(new[] {
                new AnimationTrack("particle", 0)
                    .SetFrame(0, new AnimationFilledCircle(randomX, Effects.Canvas.Height + 5, 5, Color.FromArgb(255, 230, 0)))
                    .SetFrame(1, new AnimationFilledCircle(randomX + randomOffset, -6, 6, Color.FromArgb(0, 255, 230, 0)))
            }),
            time = 0
        });
    }

    public override EffectLayer Render(IGameState gamestate) {
        // Render nothing if invalid gamestate or player isn't on fire
        if (gamestate is not GameState_Minecraft minecraft || !minecraft.Player.IsBurning)
            return EffectLayer.EmptyLayer;

        // Set the background to red
        EffectLayer.FillOver(Brushes.Red);

        // Add 3 particles every frame
        for (var i = 0; i < 3; i++)
            CreateFireParticle();

        var graphics = EffectLayer.GetGraphics();
        // Render all particles
        foreach (var particle in particles) {
            particle.mix.Draw(graphics, particle.time);
            particle.time += .1f;
        }

        // Remove any expired particles
        particles.RemoveAll(particle => particle.time >= 1);

        return EffectLayer;
    }
}

internal class FireParticle {
    internal AnimationMix mix;
    internal float time;
}