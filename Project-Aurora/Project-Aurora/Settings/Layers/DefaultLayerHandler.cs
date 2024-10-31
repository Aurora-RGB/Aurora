using System.Windows.Controls;
using AuroraRgb.EffectsEngine;
using AuroraRgb.Profiles;
using AuroraRgb.Settings.Layers.Controls;
using AuroraRgb.Settings.Overrides;

namespace AuroraRgb.Settings.Layers;

[LogicOverrideIgnoreProperty("_PrimaryColor")]
[LogicOverrideIgnoreProperty("_Opacity")]
[LogicOverrideIgnoreProperty("_Enabled")]
[LogicOverrideIgnoreProperty("_Sequence")]
[LayerHandlerMeta(Order = -1, IsDefault = true)]
public class DefaultLayerHandler : LayerHandler<LayerHandlerProperties, NoRenderLayer>
{
    protected override UserControl CreateControl()
    {
        return new Control_DefaultLayer();
    }
}