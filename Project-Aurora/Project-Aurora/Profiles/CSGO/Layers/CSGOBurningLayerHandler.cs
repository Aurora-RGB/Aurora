using System;
using System.Drawing;
using System.Windows.Controls;
using AuroraRgb.EffectsEngine;
using AuroraRgb.Profiles.CSGO.GSI;
using AuroraRgb.Settings.Layers;
using AuroraRgb.Utils;
using Newtonsoft.Json;

namespace AuroraRgb.Profiles.CSGO.Layers;

public partial class CSGOBurningLayerHandlerProperties : LayerHandlerProperties2Color
{
    private Color? _burningColor;

    [JsonProperty("_BurningColor")]
    public Color BurningColor
    {
        get => Logic?._BurningColor ?? _burningColor ?? Color.Empty;
        set => _burningColor = value;
    }

    private bool? _animated;

    [JsonProperty("_Animated")]
    public bool Animated
    {
        get => Logic?._Animated ?? _animated ?? false;
        set => _animated = value;
    }

    public CSGOBurningLayerHandlerProperties()
    { }

    public CSGOBurningLayerHandlerProperties(bool assignDefault = false) : base(assignDefault) { }

    public override void Default()
    {
        base.Default();

        _burningColor = Color.FromArgb(255, 70, 0);
        _animated = true;
    }
}

public class CSGOBurningLayerHandler() : LayerHandler<CSGOBurningLayerHandlerProperties>("CSGO - Burning")
{
    private readonly Random _randomizer = new();
    private readonly SolidBrush _solidBrush = new(Color.Empty);

    protected override UserControl CreateControl()
    {
        return new Control_CSGOBurningLayer(this);
    }

    public override EffectLayer Render(IGameState gameState)
    {
        if (gameState is not GameStateCsgo csgostate) return EffectLayer.EmptyLayer;

        //Update Burning

        if (csgostate.Player.State.Burning <= 0) return EffectLayer.EmptyLayer;
        var burnColor = Properties.BurningColor;

        if (Properties.Animated)
        {
            var redAdjusted = (int)(burnColor.R + Math.Cos((Time.GetMillisecondsSinceEpoch() + _randomizer.Next(75)) / 75.0) * 0.15 * 255);

            byte red = redAdjusted switch
            {
                > 255 => 255,
                < 0 => 0,
                _ => (byte) redAdjusted
            };

            var greenAdjusted = (int)(burnColor.G + Math.Sin((Time.GetMillisecondsSinceEpoch() + _randomizer.Next(150)) / 75.0) * 0.15 * 255);

            byte green = greenAdjusted switch
            {
                > 255 => 255,
                < 0 => 0,
                _ => (byte) greenAdjusted
            };

            var blueAdjusted = (int)(burnColor.B + Math.Cos((Time.GetMillisecondsSinceEpoch() + _randomizer.Next(225)) / 75.0) * 0.15 * 255);

            byte blue = blueAdjusted switch
            {
                > 255 => 255,
                < 0 => 0,
                _ => (byte) blueAdjusted
            };

            burnColor = Color.FromArgb(csgostate.Player.State.Burning, red, green, blue);
        }

        if (_solidBrush.Color == burnColor) return EffectLayer;
        _solidBrush.Color = burnColor;
        EffectLayer.Fill(_solidBrush);
        return EffectLayer;
    }
}