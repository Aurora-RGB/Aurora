using System.Drawing;
using SkiaSharp;

namespace AuroraRgb.BrushAdapters;

public interface IAuroraBrush
{
    public Brush GetBrush();
    
    public void SetPaint(SKPaint paint);
}