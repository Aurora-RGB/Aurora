using System;
using System.Collections.Generic;
using AuroraRgb.Settings;
using Common.Devices;

namespace AuroraRgb.EffectsEngine;

public sealed class EffectCanvas(
    int width,
    int height,
    Dictionary<DeviceKeys, BitmapRectangle> bitmapMap,
    float gridBaselineX,
    float gridBaselineY,
    float gridWidth,
    float gridHeight)
    : IEqualityComparer<EffectCanvas>, IEquatable<EffectCanvas>
{
    public int Width { get; } = width;
    public int Height { get; } = height;
    public int BiggestSize { get; } = Math.Max(width, height);

    //TODO those 4 vars are about Control. Remove htem from here
    public float GridBaselineX { get; } = gridBaselineX;
    public float GridBaselineY { get; } = gridBaselineY;
    private float GridWidth { get; } = gridWidth;
    private float GridHeight { get; } = gridHeight;

    public Dictionary<DeviceKeys, BitmapRectangle> BitmapMap { get; } = bitmapMap;

    public float WidthCenter { get; init; }
    public float HeightCenter { get; init; }

    public float EditorToCanvasWidth => Width / GridWidth;
    public float EditorToCanvasHeight => Height / GridHeight;

    /// <summary>
    /// Creates a new FreeFormObject that perfectly occupies the entire canvas.
    /// </summary>
    public FreeFormObject WholeFreeForm => new(-GridBaselineX, -GridBaselineY, GridWidth, GridHeight);

    public BitmapRectangle GetRectangle(DeviceKeys key)
    {
        return BitmapMap.TryGetValue(key, out var rect) ? rect : BitmapRectangle.EmptyRectangle;
    }

    public bool Equals(EffectCanvas? other)
    {
        return Width == other?.Width && Height == other.Height;
    }

    public override bool Equals(object? obj)
    {
        if (ReferenceEquals(null, obj)) return false;
        if (ReferenceEquals(this, obj)) return true;
        if (obj.GetType() != GetType()) return false;
        return Equals((EffectCanvas)obj);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(Width, Height);
    }

    public bool Equals(EffectCanvas? x, EffectCanvas? y)
    {
        if (ReferenceEquals(x, y)) return true;
        if (ReferenceEquals(x, null)) return false;
        if (ReferenceEquals(y, null)) return false;
        if (x.GetType() != y.GetType()) return false;
        return x.Width == y.Width && x.Height == y.Height;
    }

    public int GetHashCode(EffectCanvas obj)
    {
        return HashCode.Combine(obj.Width, obj.Height);
    }
}