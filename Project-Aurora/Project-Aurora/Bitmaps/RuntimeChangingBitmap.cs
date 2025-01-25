using AuroraRgb.Bitmaps.GdiPlus;
using AuroraRgb.Bitmaps.Skia;
using Doner.Wrap;

namespace AuroraRgb.Bitmaps;

[WrapTo(nameof(_bitmap))]
public sealed partial class RuntimeChangingBitmap : IAuroraBitmap
{
    private readonly IAuroraBitmap _bitmap;

    public RuntimeChangingBitmap(int canvasWidth, int canvasHeight, bool readable)
    {
        _bitmap = new GdiBitmap(canvasWidth, canvasHeight);
    }

    public GdiBitmap GetGdiBitmap()
    {
        if (_bitmap is GdiBitmap gdiBitmap)
        {
            return gdiBitmap;
        }
        return GdiBitmap.EmptyBitmap;
    }

    public AuroraSkiaBitmap GetSkiaCpuBitmap()
    {
        if (_bitmap is AuroraCpuSkiaBitmap skiaBitmap)
        {
            return skiaBitmap;
        }
        return AuroraCpuSkiaBitmap.EmptyBitmap;
    }

    public AuroraSkiaBitmap GetSkiaVulkanBitmap()
    {
        if (_bitmap is AuroraVulkanSkiaBitmap skiaBitmap)
        {
            return skiaBitmap;
        }
        return AuroraVulkanSkiaBitmap.EmptyBitmap;
    }
}