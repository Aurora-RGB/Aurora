using System;
using System.Drawing;
using System.Windows.Controls;
using AuroraRgb.EffectsEngine;
using AuroraRgb.Profiles;
using AuroraRgb.Settings.Layers.Controls;
using AuroraRgb.Settings.Overrides;
using AuroraRgb.Utils;
using Common.Utils;
using Newtonsoft.Json;

namespace AuroraRgb.Settings.Layers;

public class BreathingLayerHandlerProperties : LayerHandlerProperties2Color<BreathingLayerHandlerProperties>
{
    public bool? _RandomPrimaryColor { get; set; }

    [JsonIgnore]
    public bool RandomPrimaryColor => Logic?._RandomPrimaryColor ?? _RandomPrimaryColor ?? false;

    public bool? _RandomSecondaryColor { get; set; }

    [JsonIgnore]
    public bool RandomSecondaryColor => Logic?._RandomSecondaryColor ?? _RandomSecondaryColor ?? false;

    [LogicOverridable("Effect Speed")]
    public double? _EffectSpeed { get; set; }

    [JsonIgnore]
    public double EffectSpeed => Logic?._EffectSpeed ?? _EffectSpeed ?? 0.0;

    public BreathingLayerHandlerProperties()
    { }

    public BreathingLayerHandlerProperties(bool assignDefault = false) : base(assignDefault) { }

    public override void Default()
    {
        base.Default();
        _RandomPrimaryColor = false;
        _RandomSecondaryColor = false;
        _EffectSpeed = 1.0f;
    }
}

public class BreathingLayerHandler() : LayerHandler<BreathingLayerHandlerProperties>("Breathing Layer")
{
    private Color _currentPrimaryColor = Color.Transparent;
    private Color _currentSecondaryColor = Color.Transparent;

    protected override UserControl CreateControl()
    {
        return new Control_BreathingLayer(this);
    }

    public override EffectLayer Render(IGameState gameState)
    {
        var currentSine = (float)Math.Pow(Math.Sin((double)(Time.GetMillisecondsSinceEpoch() % 10000L / 10000.0f) * 2 * Math.PI * Properties.EffectSpeed), 2);

        if (currentSine <= 0.0025f * Properties.EffectSpeed && Properties.RandomSecondaryColor)
            _currentSecondaryColor = CommonColorUtils.GenerateRandomColor();
        else if(!Properties.RandomSecondaryColor)
            _currentSecondaryColor = Properties.SecondaryColor;

        if (currentSine >= 1.0f - 0.0025f * Properties.EffectSpeed && Properties.RandomPrimaryColor)
            _currentPrimaryColor = CommonColorUtils.GenerateRandomColor();
        else if (!Properties.RandomPrimaryColor)
            _currentPrimaryColor = Properties.PrimaryColor;

        EffectLayer.Set(Properties.Sequence, ColorUtils.BlendColors(_currentPrimaryColor, _currentSecondaryColor, currentSine));

        return EffectLayer;
    }
}