using System.Drawing;
using Common;
using SkiaSharp;

namespace AuroraRgb.BrushAdapters;

public sealed class SingleColorBrush(SimpleColor color) : IAuroraBrush
{
    public SimpleColor Color { get; set; } = color;

    private SolidBrush? _brush;
    private SimpleColor _previousColor;

    public SingleColorBrush() : this(SimpleColor.Transparent)
    {
    }

    public Brush GetBrush()
    {
        _brush ??= new SolidBrush(System.Drawing.Color.Transparent);

        if (Color != _previousColor)
        {
            var gdiColor = (Color)Color;
            _brush.Color = gdiColor;
            _previousColor = Color;
        }
        
        return _brush;
    }

    public void SetPaint(SKPaint paint)
    {
        paint.Reset();
        paint.Color = new SKColor((uint)Color.ToArgb());
    }

    public void Dispose()
    {
        _brush?.Dispose();
    }
}