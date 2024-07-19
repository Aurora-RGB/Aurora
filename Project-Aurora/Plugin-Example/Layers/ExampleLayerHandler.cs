using System.Windows.Controls;
using AuroraRgb.EffectsEngine;
using AuroraRgb.Profiles;
using AuroraRgb.Settings.Layers;

namespace Plugin_Example.Layers
{
    public partial class ExampleLayerHandlerProperties : LayerHandlerProperties<ExampleLayerHandlerProperties>
    {

    }

    public class ExampleLayerHandler : LayerHandler<LayerHandlerProperties>
    {
        protected override UserControl CreateControl()
        {
            return new Control_ExampleLayer(this);
        }

        public override EffectLayer Render(IGameState gamestate)
        {
            EffectLayer solidcolor_layer = new EffectLayer("ExampleLayer");
            solidcolor_layer.Set(Properties.Sequence, Properties.PrimaryColor);
            return solidcolor_layer;
        }
    }
}
