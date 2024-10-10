using System.Drawing;
using System.Drawing.Drawing2D;
using AuroraRgb.BrushAdapters;
using AuroraRgb.EffectsEngine;
using AuroraRgb.Settings;
using SkiaSharp;

namespace AuroraRgb.Bitmaps.Skia;

public abstract class AuroraSkiaBitmap : IAuroraBitmap
{
    protected static readonly SKPaint SkPaint = new();

    protected SKCanvas Canvas;

    protected readonly int _width;
    protected readonly int _height;

    public AuroraSkiaBitmap(int canvasWidth, int canvasHeight)
    {
        _width = canvasWidth;
        _height = canvasHeight;
    }

    public virtual void Dispose()
    {
        Canvas.Dispose();
    }

    protected virtual void Invalidate()
    {
    }

    public float Opacity { get; set; } = 1.0f;
    public abstract Color GetRegionColor(Rectangle keyRectangleRectangle);

    public void Reset()
    {
    }

    public void DrawRectangle(Brush brush, Rectangle dimension)
    {
        var skiaRectangle = SkiaRectangle(dimension);
        SetBrush(brush);

        Canvas.DrawRect(skiaRectangle, SkPaint);
    }

    public void DrawRectangle(Brush brush, RectangleF dimension)
    {
        var skiaRectangle = SkiaRectangle(dimension);
        SetBrush(brush);

        Canvas.DrawRect(skiaRectangle, SkPaint);
    }

    public void DrawRectangle(IAuroraBrush brush, Rectangle dimension)
    {
        var skiaRectangle = SkiaRectangle(dimension);
        DrawRectangle(brush, skiaRectangle);
    }

    public void DrawRectangle(IAuroraBrush brush, RectangleF dimension)
    {
        var skiaRectangle = SkiaRectangle(dimension);
        DrawRectangle(brush, skiaRectangle);
    }

    private void DrawRectangle(IAuroraBrush brush, SKRect skiaRectangle)
    {
        brush.SetPaint(SkPaint);
        Canvas.DrawRect(skiaRectangle, SkPaint);
    }

    public void DrawRectangle(Pen pen, RectangleF dimension)
    {
    }

    public abstract void DrawRectangle(EffectLayer brush);

    public abstract void DrawRectangle(EffectLayer brush, Rectangle dimension);

    private static void SetBrush(Brush brush)
    {
        switch (brush)
        {
            case SolidBrush solidBrush:
                SkPaint.Color = new SKColor(solidBrush.Color.R, solidBrush.Color.G, solidBrush.Color.B, solidBrush.Color.A);
                break;
            default:
                SkPaint.Color = SKColors.Empty;
                break;
        }
    }

    public void ReplaceRectangle(Brush brush, Rectangle dimension)
    {
        var skiaRectangle = SkiaRectangle(dimension);
        SetBrush(brush);

        Replace(skiaRectangle);
    }

    public void ReplaceRectangle(Brush brush, RectangleF dimension)
    {
        var skiaRectangle = SkiaRectangle(dimension);
        SetBrush(brush);

        Replace(skiaRectangle);
    }

    public void ReplaceRectangle(IAuroraBrush brush, Rectangle dimension)
    {
        var skiaRectangle = SkiaRectangle(dimension);
        brush.SetPaint(SkPaint);

        Replace(skiaRectangle);
    }

    public void ReplaceRectangle(IAuroraBrush brush, RectangleF dimension)
    {
        var skiaRectangle = SkiaRectangle(dimension);
        brush.SetPaint(SkPaint);

        Replace(skiaRectangle);
    }

    private void Replace(SKRect skiaRectangle)
    {
        SkPaint.BlendMode = SKBlendMode.Src;
        Canvas.DrawRect(skiaRectangle, SkPaint);
        SkPaint.BlendMode = SKBlendMode.SrcOver;

        Invalidate();
    }

    public void PerformExclude(KeySequence excludeSequence)
    {
    }

    public void OnlyInclude(KeySequence sequence)
    {
    }

    public void SetClip(RectangleF boundsRaw)
    {
    }

    public void SetTransform(Matrix value)
    {
    }

    public void DrawEllipse(Pen pen, RectangleF dimension)
    {
    }

    public void FillEllipse(Brush brush, RectangleF dimension)
    {
    }

    public void FillEllipse(IAuroraBrush brush, Rectangle dimension)
    {
    }

    public void FillEllipse(IAuroraBrush brush, RectangleF dimension)
    {
    }

    public void DrawImage(Image image, float x = 0, float y = 0, float width = 0, float height = 0)
    {
    }

    public void DrawLine(Pen pen, PointF startPoint, PointF endPoint)
    {
    }

    public void Fill(Brush brush)
    {
        SetBrush(brush);
        Canvas.DrawRect(0, 0, _width, _height, SkPaint);

        Invalidate();
    }

    protected static Color GetAverageColorInRectangle(SKBitmap bitmap, SKRectI rect)
    {
        // Ensure the rectangle is within the bounds of the bitmap
        rect = SKRectI.Intersect(rect, new SKRectI(0, 0, bitmap.Width, bitmap.Height));
        if (rect.IsEmpty) return Color.Transparent;

        // Create a subset of the bitmap based on the given rectangle
        using var subsetBitmap = new SKBitmap(rect.Width, rect.Height, SKImageInfo.PlatformColorType, SKAlphaType.Unpremul);
        bitmap.ExtractSubset(subsetBitmap, rect);

        // Now calculate the average color from the subset
        var pixels = subsetBitmap.Pixels;
        long red = 0, green = 0, blue = 0, alpha = 0;
        var totalPixels = pixels.Length;

        if (totalPixels == 0)
        {
            return Color.Transparent;
        }

        for (var i = 0; i < totalPixels; i++)
        {
            var color = pixels[i];
            red += color.Red;
            green += color.Green;
            blue += color.Blue;
            alpha += color.Alpha;
        }

        // Calculate the average color components
        var avgRed = (byte)(red / totalPixels);
        var avgGreen = (byte)(green / totalPixels);
        var avgBlue = (byte)(blue / totalPixels);
        var avgAlpha = (byte)(alpha / totalPixels);

        // Return the average color
        return Color.FromArgb(avgAlpha, avgRed, avgGreen, avgBlue);
    }

    protected static SKRectI SkiaRectangle(Rectangle drawingRectangle)
    {
        return new SKRectI(drawingRectangle.Left, drawingRectangle.Top, drawingRectangle.Right, drawingRectangle.Bottom);
    }

    protected static SKRect SkiaRectangle(RectangleF drawingRectangle)
    {
        return new SKRect(drawingRectangle.Left, drawingRectangle.Top, drawingRectangle.Right, drawingRectangle.Bottom);
    }
}