using System;
using System.Collections.Generic;
using AuroraRgb.Settings;
using Common.Devices;

namespace AuroraRgb.EffectsEngine;

public sealed class CanvasGridProperties(
    float gridBaselineX,
    float gridBaselineY,
    float gridWidth,
    float gridHeight)
{
    //TODO those 4 vars are about Control. Remove htem from here
    public float GridBaselineX { get; } = gridBaselineX;
    public float GridBaselineY { get; } = gridBaselineY;
    public float GridWidth { get; } = gridWidth;
    public float GridHeight { get; } = gridHeight;
}

public sealed class EffectCanvas : IEqualityComparer<EffectCanvas>, IEquatable<EffectCanvas>
{
    public EffectCanvas(int width,
        int height,
        Dictionary<DeviceKeys, BitmapRectangle> bitmapMap)
    {
        Width = width;
        Height = height;
        BiggestSize = Math.Max(width, height);
        BitmapMap = bitmapMap;
        CanvasGridProperties = new(0, 0, width, height);
        
        EntireSequence = new(WholeFreeForm);
    }

    public int Width { get; }
    public int Height { get; }
    public int BiggestSize { get; }

    //TODO those 4 vars are about Control. Remove htem from here
    public float GridBaselineX => CanvasGridProperties.GridBaselineX;
    public float GridBaselineY => CanvasGridProperties.GridBaselineY;

    public Dictionary<DeviceKeys, BitmapRectangle> BitmapMap { get; }

    public float WidthCenter { get; init; }
    public float HeightCenter { get; init; }

    public CanvasGridProperties CanvasGridProperties { get; set; }

    public float EditorToCanvasWidth => Width / CanvasGridProperties.GridWidth;
    public float EditorToCanvasHeight => Height / CanvasGridProperties.GridHeight;

    /// <summary>
    /// Creates a new FreeFormObject that perfectly occupies the entire canvas.
    /// </summary>
    public FreeFormObject WholeFreeForm => new(-CanvasGridProperties.GridBaselineX, -CanvasGridProperties.GridBaselineY, CanvasGridProperties.GridWidth, CanvasGridProperties.GridHeight);
    public KeySequence EntireSequence { get; }

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