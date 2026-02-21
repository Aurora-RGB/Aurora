using System.Globalization;
using System.Windows.Controls;
using AuroraRgb.EffectsEngine;
using AuroraRgb.Profiles;
using AuroraRgb.Settings.Layers.Controls;
using AuroraRgb.Settings.Overrides;
using Newtonsoft.Json;

namespace AuroraRgb.Settings.Layers;

public partial class PercentGradientLayerHandlerProperties : PercentLayerHandlerProperties
{
    private EffectBrush? _gradient;

    [JsonProperty("_Gradient")]
    [LogicOverridable("Gradient")]
    public EffectBrush Gradient
    {
        get => Logic?._gradient ?? (_gradient ??= new EffectBrush(EffectBrush.BrushType.Linear));
        set => _gradient = value;
    }

    public override void Default()
    {
        base.Default();
        Gradient = new EffectBrush(EffectBrush.BrushType.Linear);
    }
}

[LogicOverrideIgnoreProperty("_PrimaryColor")]
[LogicOverrideIgnoreProperty("SecondaryColor")]
[LayerHandlerMeta(Name = "Percent (Gradient)", IsDefault = true)]
public sealed class PercentGradientLayerHandler : LayerHandler<PercentGradientLayerHandlerProperties>
{
    private ZoneKeyPercentDrawer _percentDrawer;

    public PercentGradientLayerHandler() : base("PercentLayer")
    {
        _percentDrawer = new ZoneKeyPercentDrawer(EffectLayer);
    }

    protected override UserControl CreateControl()
    {
        return new Control_PercentGradientLayer(this);
    }

    public override EffectLayer Render(IGameState gameState)
    {
        EffectLayer.Clear();

        if (Invalidated)
        {
            _percentDrawer = new ZoneKeyPercentDrawer(EffectLayer);
            Invalidated = false;
        }

        var value = Properties.Logic?._Value ?? gameState.GetNumber(Properties.VariablePath);
        var maxvalue = Properties.Logic?._MaxValue ?? gameState.GetNumber(Properties.MaxVariablePath);

        _percentDrawer.PercentEffect(Properties.Gradient.GetColorSpectrum(), Properties.Sequence, value, maxvalue, Properties.PercentType,
            Properties.BlinkThreshold, Properties.BlinkDirection);
        return EffectLayer;
    }

    public override void SetApplication(Application profile)
    {
        if (!double.TryParse(Properties.VariablePath.GsiPath, CultureInfo.InvariantCulture, out _) &&
            !string.IsNullOrWhiteSpace(Properties.VariablePath.GsiPath) &&
            !profile.ParameterLookup.IsValidParameter(Properties.VariablePath.GsiPath)
           )
            Properties.VariablePath = VariablePath.Empty;

        if (!double.TryParse(Properties.MaxVariablePath.GsiPath, CultureInfo.InvariantCulture, out _) &&
            !string.IsNullOrWhiteSpace(Properties.MaxVariablePath.GsiPath) &&
            !profile.ParameterLookup.IsValidParameter(Properties.MaxVariablePath.GsiPath)
           )
            Properties.MaxVariablePath = VariablePath.Empty;
        base.SetApplication(profile);
    }

    public override void Dispose()
    {
        EffectLayer.Dispose();
        _percentDrawer.Dispose();
        base.Dispose();
    }
}