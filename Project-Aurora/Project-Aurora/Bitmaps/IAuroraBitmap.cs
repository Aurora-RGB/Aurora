using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using AuroraRgb.Bitmaps.GdiPlus;
using AuroraRgb.EffectsEngine;
using AuroraRgb.Settings;

namespace AuroraRgb.Bitmaps;

public interface IAuroraBitmap : IDisposable
{
    float Opacity { get; set; }

    Color GetRegionColor(Rectangle keyRectangleRectangle);

    void Reset();
    void DrawRectangle(Brush brush, Rectangle dimension);
    void DrawRectangle(Brush brush, RectangleF dimension);
    void DrawRectangle(Pen pen, RectangleF dimension);
    void ReplaceRectangle(Brush brush, Rectangle dimension);
    void ReplaceRectangle(Brush brush, RectangleF dimension);
    void PerformExclude(KeySequence excludeSequence);
    void OnlyInclude(KeySequence sequence);

    void SetClip(RectangleF boundsRaw);
    //TODO change to IAuroraMatrix
    void SetTransform(Matrix value);

    void DrawRectangle(EffectLayer brush);
    void DrawRectangle(EffectLayer brush, Rectangle dimension);
    void DrawEllipse(Pen pen, RectangleF dimension);
    void FillEllipse(Brush brush, RectangleF dimension);
    void DrawImage(Image image, float x = 0, float y = 0, float width = 0, float height = 0);
    void DrawLine(Pen pen, PointF startPoint, PointF endPoint);
    void Fill(Brush brush);
}

public sealed class RuntimeChangingBitmap : IAuroraBitmap
{
    private readonly IAuroraBitmap _bitmap;

    public float Opacity
    {
        get => _bitmap.Opacity;
        set => _bitmap.Opacity = value;
    }

    public RuntimeChangingBitmap(int canvasWidth, int canvasHeight)
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

    public void DrawRectangle(EffectLayer brush)
    {
        _bitmap.DrawRectangle(brush);
    }

    public void DrawRectangle(EffectLayer brush, Rectangle dimension)
    {
        _bitmap.DrawRectangle(brush, dimension);
    }

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
        return (GdiBitmap)_bitmap;
    }
}