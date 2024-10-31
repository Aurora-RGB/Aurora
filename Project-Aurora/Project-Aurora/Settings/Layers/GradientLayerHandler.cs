using System;
using System.Windows.Controls;
using AuroraRgb.Bitmaps;
using AuroraRgb.EffectsEngine;
using AuroraRgb.Profiles;
using AuroraRgb.Settings.Layers.Controls;
using AuroraRgb.Settings.Overrides;
using AuroraRgb.Utils;
using Common.Utils;
using Newtonsoft.Json;

namespace AuroraRgb.Settings.Layers;

public partial class GradientLayerHandlerProperties : LayerHandlerProperties2Color
{
    private LayerEffectConfig? _gradientConfig;

    [LogicOverridable("Gradient")]
    [JsonProperty("_GradientConfig")]
    public LayerEffectConfig GradientConfig
    {
        get => Logic?._gradientConfig ?? (_gradientConfig ??= DefaultGradientConfig());
        set => _gradientConfig = value;
    }

    public override void Default()
    {
        base.Default();
        _gradientConfig = DefaultGradientConfig();
    }

    private static LayerEffectConfig DefaultGradientConfig()
    {
        return new LayerEffectConfig(CommonColorUtils.GenerateRandomColor(), CommonColorUtils.GenerateRandomColor()) { AnimationType = AnimationType.None };
    }
}

[LogicOverrideIgnoreProperty("_PrimaryColor")]
[LogicOverrideIgnoreProperty("SecondaryColor")]
public class GradientLayerHandler : LayerHandler<GradientLayerHandlerProperties, BitmapEffectLayer>
{
    private readonly BitmapEffectLayer _tempLayerWrapped = new("GradientLayer - Colors", true);
    private readonly Action<IAuroraBitmap> _gradientRenderFunc;

    public GradientLayerHandler(): base("GradientLayer")
    {
        Properties.PropertyChanged += PropertiesChanged;
        _gradientRenderFunc = g =>
        {
            g.DrawRectangle(_tempLayerWrapped);
        };
    }

    protected override UserControl CreateControl()
    {
        return new Control_GradientLayer(this);
    }
    public override EffectLayer Render(IGameState gameState)
    {
        if (Invalidated)
        {
            EffectLayer.Clear();
            Invalidated = false;
        }
        //If Wave Size 0 Gradiant Stop Moving Animation
        if (Properties.GradientConfig.GradientSize == 0)
        {
            Properties.GradientConfig.ShiftAmount += (Time.GetMillisecondsSinceEpoch() - Properties.GradientConfig.LastEffectCall) / 1000.0f * 5.0f * Properties.GradientConfig.Speed;
            Properties.GradientConfig.ShiftAmount %= Effects.Canvas.BiggestSize;
            Properties.GradientConfig.LastEffectCall = Time.GetMillisecondsSinceEpoch();

            var selectedColor = Properties.GradientConfig.Brush.GetColorSpectrum().GetColorAt(Properties.GradientConfig.ShiftAmount, Effects.Canvas.BiggestSize);

            EffectLayer.Set(Properties.Sequence, selectedColor);
        }
        else
        {
            _tempLayerWrapped.DrawGradient(LayerEffects.GradientShift_Custom_Angle, Properties.GradientConfig);
            EffectLayer.Clear();
            EffectLayer.DrawTransformed(
                Properties.Sequence,
                _gradientRenderFunc
            );
        }
        return EffectLayer;
    }

    public override void Dispose()
    {
        Properties.PropertyChanged -= PropertiesChanged;
        EffectLayer.Dispose();
        base.Dispose();
    }
}