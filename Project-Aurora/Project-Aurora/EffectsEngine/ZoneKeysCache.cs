using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using AuroraRgb.Settings;
using AuroraRgb.Utils.Rock;
using Common.Devices;

namespace AuroraRgb.EffectsEngine;

public sealed class ZoneKeysCache : IDisposable
{
    public event EventHandler? KeysChanged;
    
    private OrderedHashSet<DeviceKeys> _zoneKeys = [];
    private KeySequence? _lastKeySequence;
    private KeySequenceType _lastKeySequenceType = KeySequenceType.Sequence;
    private FreeFormObject? _lastFreeForm;
    
    private bool _invalidated;
    
    // reuse buckets to avoid mem allocs
    private readonly OrderedHashSet<DeviceKeys> _hashSetReuse = new(4); // needs initial capacity for .Union to work

    public ZoneKeysCache()
    {
        Effects.CanvasChanged += EffectsOnCanvasChanged;
    }

    public void SetSequence(KeySequence keySequence)
    {
        if (keySequence.Equals(_lastKeySequence) && _lastKeySequenceType == keySequence.Type && !_invalidated)
        {
            return;
        }

        _zoneKeys = GetKeys(keySequence);
        _lastKeySequence = keySequence;
        _lastKeySequenceType = keySequence.Type;
        _invalidated = false;
    }

    private OrderedHashSet<DeviceKeys> WithHashset(IEnumerable<DeviceKeys> keys)
    {
        _hashSetReuse.Clear();
        _hashSetReuse.UnionWith(keys);
        return _hashSetReuse;
    }

    public OrderedHashSet<DeviceKeys> GetKeys()
    {
        if (_invalidated)
        {
            if (_lastKeySequence == null)
            {
                return [];
            }
            _zoneKeys = GetKeys(_lastKeySequence);
        }
        return _zoneKeys;
    }

    private OrderedHashSet<DeviceKeys> GetKeys(KeySequence keySequence)
    {
        return keySequence.Type switch
        {
            KeySequenceType.Sequence => WithHashset(keySequence.Keys),
            KeySequenceType.FreeForm => GetKeys(keySequence.Freeform),
            _ => throw new ArgumentOutOfRangeException(nameof(keySequence))
        };
    }

    private OrderedHashSet<DeviceKeys> GetKeys(FreeFormObject freeFormObject)
    {
        // Store the new FreeFormObject and subscribe to its changes
        if (_lastFreeForm != null)
        {
            _lastFreeForm.ValuesChanged -= FreeFormObjectOnValuesChanged;
        }
        
        _lastFreeForm = freeFormObject;
        freeFormObject.ValuesChanged += FreeFormObjectOnValuesChanged;

        // Calculate and cache the keys
        _zoneKeys = CalculateKeys(freeFormObject);
        KeysChanged?.Invoke(this, EventArgs.Empty);
        return _zoneKeys;
    }

    private OrderedHashSet<DeviceKeys> CalculateKeys(FreeFormObject freeForm)
    {
        var canvas = Effects.Canvas;

        var corners = GetCorners(freeForm);

        var matchingKeys = canvas.Keys.Where(key =>
        {
            ref readonly var rect = ref canvas.GetRectangle(key);

            // Create corners for the key rectangle
            var keyCorners = new[]
            {
                new PointF(rect.Left, rect.Bottom),
                new PointF(rect.Right, rect.Bottom),
                new PointF(rect.Right, rect.Top),
                new PointF(rect.Left, rect.Top)
            };

            return PolygonContainsPolygon(corners, keyCorners);
        });

        return WithHashset(matchingKeys);
    }

    private static PointF[] GetCorners(FreeFormObject freeForm)
    {
        // Convert FreeForm coordinates to canvas coordinates
        var xPos = (freeForm.X + Effects.Canvas.GridBaselineX) * Effects.Canvas.EditorToCanvasWidth;
        var yPos = (freeForm.Y + Effects.Canvas.GridBaselineY) * Effects.Canvas.EditorToCanvasHeight;
        var width = freeForm.Width * Effects.Canvas.EditorToCanvasWidth;
        var height = freeForm.Height * Effects.Canvas.EditorToCanvasHeight;

        var left = xPos;
        var right = xPos + width;
        var top = yPos;
        var bottom = yPos + height;

        // Calculate the center point of rotation
        var centerX = xPos + width / 2;
        var centerY = yPos + height / 2;

        // Convert angle to radians
        var angleRad = freeForm.Angle * Math.PI / 180;
        var cos = Math.Cos(angleRad);
        var sin = Math.Sin(angleRad);
 
        // Calculate the corners of the rotated rectangle
        return
        [
            TransformPoint(left, top, centerX, centerY, cos, sin),
            TransformPoint(right, top, centerX, centerY, cos, sin),
            TransformPoint(right, bottom, centerX, centerY, cos, sin),
            TransformPoint(left, bottom, centerX, centerY, cos, sin)
        ];
    }

    private readonly struct PointF(float x, float y)
    {
        public readonly float X = x;
        public readonly float Y = y;
    }

    private static PointF TransformPoint(float x, float y, float centerX, float centerY, double cos, double sin)
    {
        // Translate point to origin
        var translatedX = x - centerX;
        var translatedY = y - centerY;

        // Rotate
        var rotatedX = translatedX * cos - translatedY * sin;
        var rotatedY = translatedX * sin + translatedY * cos;

        // Translate back
        return new PointF(
            (float)(rotatedX + centerX),
            (float)(rotatedY + centerY)
        );
    }
    
    private static bool PolygonContainsPolygon(PointF[] container, [Length(4, 4)] PointF[] contained)
    {
        // First, check if any point of the contained polygon is outside the container
        return Array.TrueForAll(contained, point => PointInRectangle(point, container));
    }

    private static bool PointInRectangle(PointF point, [Length(4, 4)] PointF[] rectangleCorners)
    {
        // Translate the point and rectangle so that the rectangle's first corner is at the origin
        var translatedPoint = new PointF(
            point.X - rectangleCorners[0].X, 
            point.Y - rectangleCorners[0].Y
        );

        // Calculate the vectors of the rectangle's sides
        var vector1 = new PointF(
            rectangleCorners[1].X - rectangleCorners[0].X,
            rectangleCorners[1].Y - rectangleCorners[0].Y
        );

        var vector2 = new PointF(
            rectangleCorners[3].X - rectangleCorners[0].X,
            rectangleCorners[3].Y - rectangleCorners[0].Y
        );

        // Calculate dot products to determine if the point is inside
        var dot1 = translatedPoint.X * vector1.X + translatedPoint.Y * vector1.Y;
        var dot2 = translatedPoint.X * vector2.X + translatedPoint.Y * vector2.Y;

        // Check if the point is within the rectangle's side lengths
        const int delta = 50;
        return dot1 + delta >= 0 && dot1 - delta <= vector1.X * vector1.X + vector1.Y * vector1.Y &&
               dot2 + delta >= 0 && dot2 - delta <= vector2.X * vector2.X + vector2.Y * vector2.Y;
    }

    private void FreeFormObjectOnValuesChanged(object? sender, FreeFormChangedEventArgs e)
    {
        Invalidate();
    }

    private void EffectsOnCanvasChanged(object? sender, EventArgs e)
    {
        Invalidate();
    }

    public void Invalidate()
    {
        _invalidated = true;
    }

    public void Dispose()
    {
        Effects.CanvasChanged -= EffectsOnCanvasChanged;
    }
}