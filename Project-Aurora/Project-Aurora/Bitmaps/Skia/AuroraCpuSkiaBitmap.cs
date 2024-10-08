using System;
using System.Drawing;
using AuroraRgb.EffectsEngine;
using SkiaSharp;

namespace AuroraRgb.Bitmaps.Skia;

public class AuroraCpuSkiaBitmap : AuroraSkiaBitmap
{
    private readonly SKBitmap _bitmap;
    public static readonly AuroraCpuSkiaBitmap EmptyBitmap = new(1, 1);

    public AuroraCpuSkiaBitmap(int canvasWidth, int canvasHeight) : base(canvasWidth, canvasHeight)
    {
        _bitmap = new SKBitmap(canvasWidth, canvasHeight, SKImageInfo.PlatformColorType, SKAlphaType.Unpremul);
        Canvas = new SKCanvas(_bitmap);
    }

    public override Color GetRegionColor(Rectangle keyRectangleRectangle)
    {
        var skiaRectangle = SkiaRectangle(keyRectangleRectangle);
        return GetAverageColorInRectangle(_bitmap, skiaRectangle);
    }

    public override void DrawRectangle(EffectLayer brush)
    {
        var auroraSkiaBitmap = (AuroraCpuSkiaBitmap)GetSkiaBitmap(brush.GetBitmap());
        SkPaint.Color = new SKColor(255, 255, 255, (byte)(auroraSkiaBitmap.Opacity * 255));
        var skiaBitmap = auroraSkiaBitmap._bitmap;
        Canvas.DrawBitmap(skiaBitmap, 0, 0, SkPaint);
    }

    public override void DrawRectangle(EffectLayer brush, Rectangle dimension)
    {
        var auroraSkiaBitmap = (AuroraCpuSkiaBitmap)GetSkiaBitmap(brush.GetBitmap());
        SkPaint.Color = new SKColor(255, 255, 255, (byte)(auroraSkiaBitmap.Opacity * 255));
        var skiaBitmap = auroraSkiaBitmap._bitmap;
        var rectangle = SkiaRectangle(dimension);
        Canvas.DrawBitmap(skiaBitmap, rectangle, SkPaint);
    }

    private static AuroraSkiaBitmap GetSkiaBitmap(IAuroraBitmap bitmap)
    {
        return bitmap switch
        {
            AuroraSkiaBitmap skBitmap => skBitmap,
            RuntimeChangingBitmap runtimeChangingBitmap => runtimeChangingBitmap.GetSkiaCpuBitmap(),
            _ => throw new NotSupportedException("Only AuroraCpuSkiaBitmaps are supported.")
        };
    }
}