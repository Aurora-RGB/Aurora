﻿using System.Windows.Controls;
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
public class PercentGradientLayerHandler : PercentLayerHandler<PercentGradientLayerHandlerProperties>
{
        
    protected override UserControl CreateControl()
    {
        return new Control_PercentGradientLayer(this);
    }

    public override EffectLayer Render(IGameState state)
    {
        if (Invalidated)
        {
            EffectLayer.Clear();
        }
        Invalidated = false;
            
        var value = Properties.Logic?._Value ?? state.GetNumber(Properties.VariablePath);
        var maxvalue = Properties.Logic?._MaxValue ?? state.GetNumber(Properties.MaxVariablePath);

        EffectLayer.PercentEffect(Properties.Gradient.GetColorSpectrum(), Properties.Sequence, value, maxvalue, Properties.PercentType, Properties.BlinkThreshold, Properties.BlinkDirection);
        return EffectLayer;
    }

    public override void Dispose()
    {
        EffectLayer.Dispose();
        base.Dispose();
    }
}