using System.ComponentModel;
using System.Windows.Controls;
using AuroraRgb.BrushAdapters;
using AuroraRgb.EffectsEngine;
using AuroraRgb.Profiles;
using AuroraRgb.Settings.Layers.Controls;
using Common;

namespace AuroraRgb.Settings.Layers;

public class SolidColorLayerHandler() : LayerHandler<LayerHandlerProperties>("SolidColorLayer")
{
    private KeySequence _propertiesSequence = new();

    protected override UserControl CreateControl()
    {
        return new Control_SolidColorLayer(this);
    }

    public override EffectLayer Render(IGameState gameState)
    {
        EffectLayer.Set(_propertiesSequence, Properties.PrimaryColor);
        return EffectLayer;
    }

    protected override void PropertiesChanged(object? sender, PropertyChangedEventArgs args)
    {
        base.PropertiesChanged(sender, args);
        _propertiesSequence = Properties.Sequence;
    }
}