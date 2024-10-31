using System.Windows.Controls;
using AuroraRgb.BrushAdapters;
using AuroraRgb.EffectsEngine;
using AuroraRgb.Profiles;
using AuroraRgb.Settings.Layers.Controls;
using AuroraRgb.Settings.Overrides;
using Common;
using Common.Utils;

namespace AuroraRgb.Settings.Layers;

public partial class SolidFillLayerHandlerProperties : LayerHandlerProperties
{

    public override void Default()
    {
        base.Default();
        _PrimaryColor = CommonColorUtils.GenerateRandomColor();
    }
}

[LogicOverrideIgnoreProperty("_Sequence")]
public sealed class SolidFillLayerHandler() : LayerHandler<SolidFillLayerHandlerProperties>("SolidFillLayerHandler")
{
    protected override UserControl CreateControl()
    {
        return new Control_SolidFillLayer(this);
    }

    public override EffectLayer Render(IGameState gameState)
    {
        EffectLayer.Fill(Properties.PrimaryColor);
        return EffectLayer;
    }
}