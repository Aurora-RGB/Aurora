using System.Drawing;
using System.Drawing.Drawing2D;
using AuroraRgb.BrushAdapters;
using AuroraRgb.EffectsEngine;

namespace AuroraRgb.Bitmaps.GdiPlus;

public interface IGdiBitmap
{
    public double Opacity { get; set; }
    public Rectangle Dimension { get; }
    public TextureBrush TextureBrush { get; }

    void DrawRectangle(Brush brush, Rectangle dimension, bool overwriteColor = false);
    void DrawRectangle(Brush brush, RectangleF dimension, bool overwriteColor = false);
    void DrawRectangle(Pen pen, RectangleF dimension);
    void DrawRectangle(EffectLayer brush);
    void DrawRectangle(IAuroraBrush brush, RectangleF dimension);
    void DrawRectangle(IAuroraBrush brush, Rectangle dimension);

    void SetTransform(Matrix value);
    void DrawEllipse(Pen pen, RectangleF dimension);
}