using System.Drawing;
using SkiaSharp;

namespace AuroraRgb.Bitmaps.Skia;

public sealed class SkiaBitmapReader(SKBitmap bitmap) : IBitmapReader
{
    private readonly SKColor[] _pixels = bitmap.Pixels;

    public Color GetRegionColor(Rectangle rectangle)
    {
        var skiaRectangle = SkiaUtils.SkiaRectangle(rectangle);
        return GetAverageColorInRectangle(skiaRectangle);
    }

    private Color GetAverageColorInRectangle(SKRectI rect)
    {
        // Ensure the rectangle is within the bounds of the bitmap
        var bitmapWidth = bitmap.Width;
        rect = SKRectI.Intersect(rect, new SKRectI(0, 0, bitmapWidth, bitmap.Height));
        if (rect.IsEmpty) return Color.Transparent;

        // Now calculate the average color from the subset
        long red = 0, green = 0, blue = 0, alpha = 0;
        var area = rect.Width * rect.Height;

        for (var y = rect.Top; y < rect.Bottom; y++)
        {
            for (var x = rect.Left; x < rect.Right; x++)
            {
                var i = y * bitmapWidth + x;
                var color = _pixels[i];
                red += color.Red;
                green += color.Green;
                blue += color.Blue;
                alpha += color.Alpha;
            }
        }

        // Calculate the average color components
        var avgRed = (byte)(red / area);
        var avgGreen = (byte)(green / area);
        var avgBlue = (byte)(blue / area);
        var avgAlpha = (byte)(alpha / area);

        // Return the average color
        return Color.FromArgb(avgAlpha, avgRed, avgGreen, avgBlue);
    }

    public void Dispose()
    {
    }
}