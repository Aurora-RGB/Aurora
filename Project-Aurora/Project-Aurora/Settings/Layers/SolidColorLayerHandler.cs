using System.ComponentModel;
using System.Drawing;
using System.Windows.Controls;
using AuroraRgb.EffectsEngine;
using AuroraRgb.Profiles;
using AuroraRgb.Settings.Layers.Controls;

namespace AuroraRgb.Settings.Layers;

public class SolidColorLayerHandler : LayerHandler<LayerHandlerProperties>
{
    private readonly SolidBrush _brush;
    private KeySequence _propertiesSequence = new();

    public SolidColorLayerHandler(): base("SolidColorLayer")
    {
        _brush = new SolidBrush(Properties.PrimaryColor);
    }

    protected override UserControl CreateControl()
    {
        return new Control_SolidColorLayer(this);
    }
        
    public override EffectLayer Render(IGameState gamestate)
    {
        EffectLayer.Set(_propertiesSequence, _brush);
        return EffectLayer;
    }

    protected override void PropertiesChanged(object? sender, PropertyChangedEventArgs args)
    {
        base.PropertiesChanged(sender, args);
        _brush.Color = Properties.PrimaryColor;
        _propertiesSequence = Properties.Sequence;
        EffectLayer.Invalidate();
    }

    public override void Dispose()
    {
        base.Dispose();
        
        _brush.Dispose();
    }
}