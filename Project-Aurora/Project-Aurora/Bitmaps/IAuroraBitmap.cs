using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using AuroraRgb.BrushAdapters;
using AuroraRgb.EffectsEngine;
using AuroraRgb.Settings;

namespace AuroraRgb.Bitmaps;

public interface IAuroraBitmap : IDisposable
{
    float Opacity { get; set; }

    IBitmapReader CreateReader();

    void Reset();
    void DrawRectangle(Brush brush, Rectangle dimension);
    void DrawRectangle(Brush brush, RectangleF dimension);
    void DrawRectangle(Pen pen, RectangleF dimension);
    void DrawRectangle(EffectLayer brush);
    void DrawRectangle(EffectLayer brush, Rectangle dimension);
    void DrawRectangle(IAuroraBrush brush, Rectangle dimension);
    void DrawRectangle(IAuroraBrush brush, RectangleF dimension);
    void ReplaceRectangle(Brush brush, Rectangle dimension);
    void ReplaceRectangle(Brush brush, RectangleF dimension);
    void ReplaceRectangle(IAuroraBrush brush, Rectangle dimension);
    void ReplaceRectangle(IAuroraBrush brush, RectangleF dimension);
    void PerformExclude(KeySequence excludeSequence);
    void OnlyInclude(KeySequence sequence);

    void SetClip(RectangleF boundsRaw);
    //TODO change to IAuroraMatrix
    void SetTransform(Matrix value);

    void DrawEllipse(Pen pen, RectangleF dimension);
    void FillEllipse(Brush brush, RectangleF dimension);
    void FillEllipse(IAuroraBrush brush, Rectangle dimension);
    void FillEllipse(IAuroraBrush brush, RectangleF dimension);
    void DrawImage(Image image, float x = 0, float y = 0, float width = 0, float height = 0);
    void DrawLine(Pen pen, PointF startPoint, PointF endPoint);
    void Fill(Brush brush);
}