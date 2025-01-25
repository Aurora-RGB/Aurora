using AuroraRgb.Bitmaps;
using Doner.Wrap;

namespace AuroraRgb.EffectsEngine;

[WrapTo(nameof(_effectLayer))]
public sealed partial class RuntimeChangingLayer : EffectLayer
{
    private readonly NoRenderLayer _noRenderLayer = new();
    private readonly BitmapEffectLayer _bitmapEffectLayer;

    private EffectLayer _effectLayer;

    public RuntimeChangingLayer(string name)
    {
        _bitmapEffectLayer = new BitmapEffectLayer(name, true);
        _effectLayer = _noRenderLayer;
    }

    public void ChangeToNoRenderLayer()
    {
        _effectLayer = _noRenderLayer;
    }

    public void ChangeToBitmapEffectLayer()
    {
        _effectLayer = _bitmapEffectLayer;
    }

    public IAuroraBitmap GetBitmap()
    {
        return _bitmapEffectLayer.GetBitmap();
    }
}