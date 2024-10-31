using System;
using System.Collections.Generic;
using System.Drawing;

namespace AuroraRgb.EffectsEngine;

public sealed class BitmapRectangle : IEqualityComparer<BitmapRectangle>, IEquatable<BitmapRectangle>
{
    public static readonly BitmapRectangle EmptyRectangle = new();

    private readonly Rectangle _rectangle;
    public ref readonly Rectangle Rectangle => ref _rectangle;

    public bool IsEmpty => _rectangle.IsEmpty;

    public int Bottom => _rectangle.Bottom;
    public int Top => _rectangle.Top;
    public int Left => _rectangle.Left;
    public int Right => _rectangle.Right;
    public int Height => _rectangle.Height;
    public int Width => _rectangle.Width;

    public PointF Center { get; }

    private BitmapRectangle()
    {
    }

    public BitmapRectangle(int x, int y, int width, int height)
    {
        _rectangle = new Rectangle(x, y, width, height);
        Center = new PointF(_rectangle.Left + _rectangle.Width / 2.0f, _rectangle.Top + _rectangle.Height / 2.0f);
    }

    public BitmapRectangle(Rectangle region)
    {
        _rectangle = new Rectangle(region.Location, region.Size);
        Center = new PointF(_rectangle.Left + _rectangle.Width / 2.0f, _rectangle.Top + _rectangle.Height / 2.0f);
    }

    public static explicit operator BitmapRectangle(Rectangle region)
    {
        return new BitmapRectangle(region);
    }

    public bool Equals(BitmapRectangle? p)
    {
        if (ReferenceEquals(null, p)) return false;
        if (ReferenceEquals(this, p)) return true;
        return _rectangle.Equals(p._rectangle) && Center.Equals(p.Center);
    }

    public override bool Equals(object? obj)
    {
        return ReferenceEquals(this, obj) || obj is BitmapRectangle other && Equals(other);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(_rectangle, Center);
    }

    public bool Equals(BitmapRectangle? x, BitmapRectangle? y)
    {
        if (ReferenceEquals(x, y)) return true;
        if (ReferenceEquals(x, null)) return false;
        if (ReferenceEquals(y, null)) return false;
        if (x.GetType() != y.GetType()) return false;
        return x._rectangle.Equals(y._rectangle) && x.Center.Equals(y.Center);
    }

    public int GetHashCode(BitmapRectangle obj)
    {
        return HashCode.Combine(obj._rectangle, obj.Center);
    }
}