using System.Diagnostics;
using System.Drawing;
using System.Windows.Controls;
using AuroraRgb.EffectsEngine;
using AuroraRgb.Profiles;
using AuroraRgb.Settings.Layers.Controls;
using AuroraRgb.Settings.Overrides;
using AuroraRgb.Utils;
using Newtonsoft.Json;

namespace AuroraRgb.Settings.Layers;

public partial class RadialLayerProperties : LayerHandlerProperties {

    private static readonly SegmentedRadialBrushFactory DefaultFactory = new(new ColorStopCollection(
        [Color.Red, Color.Orange, Color.Yellow, Color.Lime, Color.Cyan, Color.Blue, Color.Purple, Color.Red]));

    private SegmentedRadialBrushFactory? _brush;
    [JsonProperty("_Brush")]
    public SegmentedRadialBrushFactory Brush
    {
        get => Logic?._brush ?? _brush ?? DefaultFactory;
        set => _brush = value;
    }

    // Number of degrees per second the brush rotates at.
    private int? _animationSpeed;
    [LogicOverridable("Animation Speed")] 
    [JsonProperty("_AnimationSpeed")]
    public int AnimationSpeed
    {
        get => Logic?._animationSpeed ?? _animationSpeed ?? 60;
        set => _animationSpeed = value;
    }

    public override void Default() {
        base.Default();
        _Sequence = new KeySequence(Effects.Canvas.WholeFreeForm);
        _brush = (SegmentedRadialBrushFactory)DefaultFactory.Clone();
        _animationSpeed = 60;
    }
}

public class RadialLayerHandler() : LayerHandler<RadialLayerProperties, BitmapEffectLayer>("RadialLayer")
{
    private readonly Stopwatch _sw = new();
    private float _angle;

    protected override UserControl CreateControl() => new Control_RadialLayer(this);

    public override EffectLayer Render(IGameState gameState) {
        if (Invalidated)
        {
            EffectLayer.Clear();
            Invalidated = false;
        }
            
        // Calculate delta time
        var dt = _sw.Elapsed.TotalSeconds;
        _sw.Restart();

        // Update angle
        _angle = (_angle + (float)(dt * Properties.AnimationSpeed)) % 360;

        var area = Properties.Sequence.GetAffectedRegion();
        var brush = Properties.Brush.GetBrush(area, _angle);
        EffectLayer.Set(Properties.Sequence, brush);
        return EffectLayer;
    }
}