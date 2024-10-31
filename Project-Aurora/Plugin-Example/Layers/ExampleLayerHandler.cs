using System.Windows.Controls;
using AuroraRgb.EffectsEngine;
using AuroraRgb.Profiles;
using AuroraRgb.Settings.Layers;

namespace Plugin_Example.Layers
{
    public partial class ExampleLayerHandlerProperties : LayerHandlerProperties
    {

    }

    public class ExampleLayerHandler() : LayerHandler<LayerHandlerProperties>("ExampleLayer")
    {
        protected override UserControl CreateControl()
        {
            return new Control_ExampleLayer(this);
        }

        public override EffectLayer Render(IGameState gamestate)
        {
            EffectLayer.Set(Properties.Sequence, Properties.PrimaryColor);
            return EffectLayer;
        }
    }
}
