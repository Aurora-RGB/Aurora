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
public sealed class SolidFillLayerHandler : LayerHandler<SolidFillLayerHandlerProperties>
{
    private readonly SingleColorBrush _solidBrush = new(SimpleColor.Transparent);

    protected override UserControl CreateControl()
    {
        return new Control_SolidFillLayer(this);
    }

    public override EffectLayer Render(IGameState gameState)
    {
        _solidBrush.Color = (SimpleColor)Properties.PrimaryColor;
        EffectLayer.Set(Effects.Canvas.EntireSequence, _solidBrush);
        return EffectLayer;
    }
}