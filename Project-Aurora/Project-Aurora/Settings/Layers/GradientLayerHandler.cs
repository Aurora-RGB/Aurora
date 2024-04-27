using System.ComponentModel;
using System.Windows.Controls;
using AuroraRgb.EffectsEngine;
using AuroraRgb.Profiles;
using AuroraRgb.Settings.Layers.Controls;
using AuroraRgb.Settings.Overrides;
using AuroraRgb.Utils;
using Common.Utils;
using Newtonsoft.Json;

namespace AuroraRgb.Settings.Layers;

public class GradientLayerHandlerProperties : LayerHandlerProperties2Color<GradientLayerHandlerProperties>
{
    private LayerEffectConfig? _gradientConfig;

    [LogicOverridable("Gradient")]
    [JsonProperty("_GradientConfig")]
    public LayerEffectConfig GradientConfig
    {
        get => Logic?._gradientConfig ?? (_gradientConfig ??= DefaultGradientConfig());
        set => _gradientConfig = value;
    }

    public GradientLayerHandlerProperties()
    { }

    public GradientLayerHandlerProperties(bool assignDefault = false) : base(assignDefault) { }

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
[LogicOverrideIgnoreProperty("_SecondaryColor")]
public class GradientLayerHandler : LayerHandler<GradientLayerHandlerProperties>
{
    private readonly EffectLayer _tempLayerBitmap = new("GradientLayer - Colors", true);
    private bool _invalidated;

    public GradientLayerHandler(): base("GradientLayer")
    {
        Properties.PropertyChanged += PropertiesChanged;
    }

    protected override void PropertiesChanged(object? sender, PropertyChangedEventArgs args)
    {
        base.PropertiesChanged(sender, args);
        _invalidated = true;
    }

    protected override UserControl CreateControl()
    {
        return new Control_GradientLayer(this);
    }
    public override EffectLayer Render(IGameState gamestate)
    {
        if (_invalidated)
        {
            EffectLayer.Clear();
            _invalidated = false;
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
            _tempLayerBitmap.DrawGradient(LayerEffects.GradientShift_Custom_Angle, Properties.GradientConfig);
            EffectLayer.Clear();
            EffectLayer.DrawTransformed(
                Properties.Sequence,
                g =>
                {
                    g.FillRectangle(_tempLayerBitmap.TextureBrush, _tempLayerBitmap.Dimension);
                }
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