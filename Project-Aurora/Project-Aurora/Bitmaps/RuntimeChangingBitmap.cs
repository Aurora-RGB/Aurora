using System.Drawing;
using System.Drawing.Drawing2D;
using AuroraRgb.Bitmaps.GdiPlus;
using AuroraRgb.Bitmaps.Skia;
using AuroraRgb.BrushAdapters;
using AuroraRgb.EffectsEngine;
using AuroraRgb.Settings;

namespace AuroraRgb.Bitmaps;

public sealed class RuntimeChangingBitmap : IAuroraBitmap
{
    private readonly IAuroraBitmap _bitmap;

    public float Opacity
    {
        get => _bitmap.Opacity;
        set => _bitmap.Opacity = value;
    }

    public RuntimeChangingBitmap(int canvasWidth, int canvasHeight, bool readable)
    {
        _bitmap = new GdiBitmap(canvasWidth, canvasHeight);
    }

    public Color GetRegionColor(Rectangle keyRectangleRectangle)
    {
        return _bitmap.GetRegionColor(keyRectangleRectangle);
    }

    public void Reset()
    {
        _bitmap.Reset();
    }

    public void DrawRectangle(Brush brush, Rectangle dimension)
    {
        _bitmap.DrawRectangle(brush, dimension);
    }

    public void DrawRectangle(Brush brush, RectangleF dimension)
    {
        _bitmap.DrawRectangle(brush, dimension);
    }

    public void DrawRectangle(Pen pen, RectangleF dimension)
    {
        _bitmap.DrawRectangle(pen, dimension);
    }

    public void DrawRectangle(IAuroraBrush brush, RectangleF dimension)
    {
        _bitmap.DrawRectangle(brush, dimension);
    }

    public void DrawRectangle(EffectLayer brush)
    {
        _bitmap.DrawRectangle(brush);
    }

    public void DrawRectangle(EffectLayer brush, Rectangle dimension)
    {
        _bitmap.DrawRectangle(brush, dimension);
    }

    public void DrawRectangle(IAuroraBrush brush, Rectangle dimension)
    {
        _bitmap.DrawRectangle(brush, dimension);
    }

    public void ReplaceRectangle(Brush brush, Rectangle dimension)
    {
        _bitmap.ReplaceRectangle(brush, dimension);
    }

    public void ReplaceRectangle(Brush brush, RectangleF dimension)
    {
        _bitmap.ReplaceRectangle(brush, dimension);
    }

    public void PerformExclude(KeySequence excludeSequence)
    {
        _bitmap.PerformExclude(excludeSequence);
    }

    public void OnlyInclude(KeySequence sequence)
    {
        _bitmap.OnlyInclude(sequence);
    }

    public void SetClip(RectangleF boundsRaw) => _bitmap.SetClip(boundsRaw);

    public void SetTransform(Matrix value) => _bitmap.SetTransform(value);

    public void DrawEllipse(Pen pen, RectangleF dimension)
    {
        _bitmap.DrawEllipse(pen, dimension);
    }

    public void FillEllipse(Brush pen, RectangleF dimension)
    {
        _bitmap.FillEllipse(pen, dimension);
    }

    public void DrawImage(Image image, float x = 0, float y = 0, float width = 0, float height = 0)
    {
        _bitmap.DrawImage(image, x, y, width, height);
    }

    public void DrawLine(Pen pen, PointF startPoint, PointF endPoint)
    {
        _bitmap.DrawLine(pen, startPoint, endPoint);
    }

    public void Fill(Brush brush)
    {
        _bitmap.Fill(brush);
    }

    public void Dispose()
    {
        _bitmap.Dispose();
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