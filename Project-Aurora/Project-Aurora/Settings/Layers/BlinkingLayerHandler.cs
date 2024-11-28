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

public partial class BlinkingLayerHandlerProperties : LayerHandlerProperties2Color
{
    [JsonIgnore]
    private bool? _randomPrimaryColor;

    [JsonProperty("_RandomPrimaryColor")]
    public bool RandomPrimaryColor
    {
        get => Logic?._randomPrimaryColor ?? _randomPrimaryColor ?? false;
        set => _randomPrimaryColor = value;
    }

    [JsonIgnore]
    private bool? _randomSecondaryColor;

    [JsonProperty("_RandomSecondaryColor")]
    public bool RandomSecondaryColor
    {
        get => Logic?._randomSecondaryColor ?? _randomSecondaryColor ?? false;
        set => _randomSecondaryColor = value;
    }

    [JsonIgnore]
    private float? _effectSpeed;

    [JsonProperty("_EffectSpeed")]
    [LogicOverridable("Effect Speed")] 
    public float EffectSpeed
    {
        get => Logic?._effectSpeed ?? _effectSpeed ?? 0.0f;
        set => _effectSpeed = value;
    }

    public override void Default()
    {
        base.Default();
        _randomPrimaryColor = false;
        _randomSecondaryColor = false;
        _effectSpeed = 1.0f;
    }
}

public class BlinkingLayerHandler() : LayerHandler<BlinkingLayerHandlerProperties>("Blinking Layer")
{

    private Color _currentPrimaryColor = Color.Transparent;
    private Color _currentSecondaryColor = Color.Transparent;

    protected override UserControl CreateControl()
    {
        return new Control_BlinkingLayer(this);
    }

    public override EffectLayer Render(IGameState gamestate)
    {
        var currentSine = Math.Round(Math.Pow(Math.Sin(Time.GetMillisecondsSinceEpoch() % 10000L / 10000.0d * 2 * Math.PI * Properties.EffectSpeed), 2));

        if (Properties.RandomSecondaryColor && currentSine == 0.0f)
            _currentSecondaryColor = CommonColorUtils.GenerateRandomColor();
        else if(!Properties.RandomSecondaryColor)
            _currentSecondaryColor = Properties.SecondaryColor;

        if (Properties.RandomPrimaryColor && currentSine >= 0.99f)
            _currentPrimaryColor = CommonColorUtils.GenerateRandomColor();
        else if (!Properties.RandomPrimaryColor)
            _currentPrimaryColor = Properties.PrimaryColor;

        EffectLayer.Clear();
        EffectLayer.Set(Properties.Sequence, ColorUtils.BlendColors(_currentPrimaryColor, _currentSecondaryColor, currentSine));

        return EffectLayer;
    }
}