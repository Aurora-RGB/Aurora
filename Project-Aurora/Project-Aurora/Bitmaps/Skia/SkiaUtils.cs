using System.Drawing;
using SkiaSharp;

namespace AuroraRgb.Bitmaps.Skia;

internal static class SkiaUtils
{
    internal static SKRectI SkiaRectangle(Rectangle drawingRectangle)
    {
        return new SKRectI(drawingRectangle.Left, drawingRectangle.Top, drawingRectangle.Right, drawingRectangle.Bottom);
    }
}