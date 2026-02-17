using System;
using System.Collections.Frozen;
using System.Collections.Generic;
using System.Linq;
using AuroraRgb.Settings;
using Common;
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
    private CanvasGridProperties _canvasGridProperties;

    public int Width { get; }
    public int Height { get; }
    public int BiggestSize { get; }

    //TODO those 4 vars are about Control. Remove htem from here
    public float GridBaselineX => CanvasGridProperties.GridBaselineX;
    public float GridBaselineY => CanvasGridProperties.GridBaselineY;

    public FrozenDictionary<DeviceKeys, BitmapRectangle> BitmapMap { get; }
    public DeviceKeys[] Keys { get; }

    public float WidthCenter { get; init; }
    public float HeightCenter { get; init; }

    private readonly BitmapRectangle _emptyRectangle = BitmapRectangle.EmptyRectangle;
    private readonly BitmapRectangle[] _keyRectangles = Enumerable.Range(0, Constants.MaxKeyId + 1)
        .Select(_ => BitmapRectangle.EmptyRectangle)
        .ToArray();

    public CanvasGridProperties CanvasGridProperties
    {
        get => _canvasGridProperties;
        set
        {
            _canvasGridProperties = value;
            EntireSequence = new(WholeFreeForm);
        }
    }

    public float EditorToCanvasWidth => Width / CanvasGridProperties.GridWidth;
    public float EditorToCanvasHeight => Height / CanvasGridProperties.GridHeight;

    /// <summary>
    /// Creates a new FreeFormObject that perfectly occupies the entire canvas.
    /// </summary>
    public FreeFormObject WholeFreeForm => new(-CanvasGridProperties.GridBaselineX, -CanvasGridProperties.GridBaselineY, CanvasGridProperties.GridWidth, CanvasGridProperties.GridHeight);
    public KeySequence EntireSequence { get; private set; }

    public EffectCanvas(int width,
        int height,
        Dictionary<DeviceKeys, BitmapRectangle> bitmapMap)
    {
        Width = width;
        Height = height;
        BiggestSize = Math.Max(width, height);
        BitmapMap = bitmapMap.ToFrozenDictionary();
        foreach (var (key, value) in bitmapMap)
        {
            _keyRectangles[(int)key] = value;
        }
        Keys = bitmapMap.Keys.ToArray();
        CanvasGridProperties = new(0, 0, width, height);
        
        EntireSequence = new(WholeFreeForm);
    }

    public ref readonly BitmapRectangle GetRectangle(DeviceKeys key)
    {
        if (key < 0)
        {
            return ref _emptyRectangle;
        }
 
        return ref _keyRectangles[(int)key];
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