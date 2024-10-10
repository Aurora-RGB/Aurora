using System;
using System.Drawing;
using SkiaSharp;

namespace AuroraRgb.BrushAdapters;

public interface IAuroraBrush : IDisposable
{
    public Brush GetBrush();
    
    public void SetPaint(SKPaint paint);
}