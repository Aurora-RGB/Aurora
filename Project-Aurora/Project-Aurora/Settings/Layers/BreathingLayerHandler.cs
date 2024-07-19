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

public partial class BreathingLayerHandlerProperties : LayerHandlerProperties2Color<BreathingLayerHandlerProperties>
{
    public bool? _RandomPrimaryColor { get; set; }

    [JsonIgnore]
    public bool RandomPrimaryColor => Logic?._RandomPrimaryColor ?? _RandomPrimaryColor ?? false;

    public bool? _RandomSecondaryColor { get; set; }

    [JsonIgnore]
    public bool RandomSecondaryColor => Logic?._RandomSecondaryColor ?? _RandomSecondaryColor ?? false;

    private double? _effectSpeed;

    [JsonProperty("_EffectSpeed")]
    [LogicOverridable("Effect Speed")]
    public double EffectSpeed
    {
        get => Logic?._EffectSpeed ?? _effectSpeed ?? 0.0;
        set => _effectSpeed = value;
    }

    private CurveFunction? _curveFunction;

    [LogicOverridable]
    public CurveFunction CurveFunction
    {
        get => Logic?._curveFunction ?? _curveFunction ?? CurveFunction.SineSquared;
        set => _curveFunction = value;
    }

    public BreathingLayerHandlerProperties()
    {
    }

    public BreathingLayerHandlerProperties(bool assignDefault = false) : base(assignDefault)
    {
    }

    public override void Default()
    {
        base.Default();
        _RandomPrimaryColor = false;
        _RandomSecondaryColor = false;
        _effectSpeed = 1.0f;
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
        var seconds = (DateTime.UtcNow - DateTime.UnixEpoch).TotalSeconds * Properties.EffectSpeed;

        // https://www.desmos.com/calculator/yxhba9jczx
        var x = seconds % 10 < 5 ? IncreasingFunc(seconds) : DecreasingFunc(seconds);

        var smoothed = CurveFunctions.Functions[Properties.CurveFunction](x);

        if (smoothed <= 0.0025f * Properties.EffectSpeed && Properties.RandomSecondaryColor)
            _currentSecondaryColor = CommonColorUtils.GenerateRandomColor();
        else if (!Properties.RandomSecondaryColor)
            _currentSecondaryColor = Properties.SecondaryColor;

        if (smoothed >= 1.0f - 0.0025f * Properties.EffectSpeed && Properties.RandomPrimaryColor)
            _currentPrimaryColor = CommonColorUtils.GenerateRandomColor();
        else if (!Properties.RandomPrimaryColor)
            _currentPrimaryColor = Properties.PrimaryColor;

        EffectLayer.Set(Properties.Sequence, ColorUtils.BlendColors(_currentPrimaryColor, _currentSecondaryColor, smoothed));

        return EffectLayer;
    }

    private static double IncreasingFunc(double x)
    {
        return x/5 - Math.Floor(x/5);
    }

    private static double DecreasingFunc(double x)
    {
        return -x/5 + 1 + Math.Floor(x/5);
    }
}