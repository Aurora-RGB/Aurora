using System.ComponentModel;
using System.Drawing;
using System.Windows.Controls;
using AuroraRgb.EffectsEngine;
using AuroraRgb.Profiles;
using AuroraRgb.Settings.Layers.Controls;

namespace AuroraRgb.Settings.Layers;

public class SolidColorLayerHandler() : LayerHandler<LayerHandlerProperties>("SolidColorLayer")
{
    private KeySequence _propertiesSequence = new();
    private Color _color = Color.Transparent;

    protected override UserControl CreateControl()
    {
        return new Control_SolidColorLayer(this);
    }

    public override EffectLayer Render(IGameState gameState)
    {
        EffectLayer.Clear();
        EffectLayer.Set(_propertiesSequence, in _color);
        return EffectLayer;
    }

    protected override void PropertiesChanged(object? sender, PropertyChangedEventArgs args)
    {
        base.PropertiesChanged(sender, args);
        _propertiesSequence = Properties.Sequence;
        _color = Properties.PrimaryColor;
    }
}