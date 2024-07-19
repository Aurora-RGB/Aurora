using System.Drawing;
using System.Windows.Controls;
using AuroraRgb.EffectsEngine;
using AuroraRgb.Profiles;
using AuroraRgb.Settings.Layers.Controls;
using Common.Utils;

namespace AuroraRgb.Settings.Layers;

public partial class SolidFillLayerHandlerProperties : LayerHandlerProperties<SolidFillLayerHandlerProperties>
{
    public SolidFillLayerHandlerProperties()
    {
    }

    public SolidFillLayerHandlerProperties(bool arg = false) : base(arg)
    {
    }

    public override void Default()
    {
        base.Default();
        _PrimaryColor = CommonColorUtils.GenerateRandomColor();
    }
}

[Overrides.LogicOverrideIgnoreProperty("_Sequence")]
public sealed class SolidFillLayerHandler : LayerHandler<SolidFillLayerHandlerProperties>
{
    private readonly SolidBrush _solidBrush = new(Color.Transparent);

    protected override UserControl CreateControl()
    {
        return new Control_SolidFillLayer(this);
    }

    public override EffectLayer Render(IGameState gameState)
    {
        _solidBrush.Color = Properties.PrimaryColor;
        EffectLayer.Set(Effects.Canvas.EntireSequence, _solidBrush);
        return EffectLayer;
    }
}