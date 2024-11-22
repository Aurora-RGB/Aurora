using System;
using System.Linq;
using AuroraRgb.Settings;
using Common.Devices;

namespace AuroraRgb.EffectsEngine;

public sealed class ZoneKeysCache : IDisposable
{
    public event EventHandler? KeysChanged;
    
    private DeviceKeys[] _zoneKeys = [];
    private KeySequence? _lastKeySequence;
    private FreeFormObject? _lastFreeForm;
    
    private bool _invalidated;

    public ZoneKeysCache()
    {
        Effects.CanvasChanged += EffectsOnCanvasChanged;
    }

    public void SetSequence(KeySequence keySequence)
    {
        if (keySequence.Equals(_lastKeySequence) && !_invalidated)
        {
            return;
        }

        _zoneKeys = GetKeys(keySequence);
        _lastKeySequence = keySequence;
        _invalidated = false;
    }

    public DeviceKeys[] GetKeys()
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

    private DeviceKeys[] GetKeys(KeySequence keySequence)
    {
        return keySequence.Type switch
        {
            KeySequenceType.Sequence => keySequence.Keys.ToArray(),
            KeySequenceType.FreeForm => GetKeys(keySequence.Freeform),
            _ => throw new ArgumentOutOfRangeException(nameof(keySequence))
        };
    }

    private DeviceKeys[] GetKeys(FreeFormObject freeFormObject)
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

    private static DeviceKeys[] CalculateKeys(FreeFormObject freeForm)
    {
        var canvas = Effects.Canvas;

        var editorToCanvasWidth = canvas.EditorToCanvasWidth;
        var editorToCanvasHeight = canvas.EditorToCanvasHeight;

        // Convert FreeForm coordinates to canvas coordinates
        var canvasX = (freeForm.X + canvas.CanvasGridProperties.GridBaselineX) * editorToCanvasWidth;
        var canvasY = (freeForm.Y + canvas.CanvasGridProperties.GridBaselineY) * editorToCanvasHeight;
        var canvasWidth = freeForm.Width * editorToCanvasWidth;
        var canvasHeight = freeForm.Height * editorToCanvasHeight;

        // Calculate the center point of rotation
        var centerX = canvasX + canvasWidth / 2;
        var centerY = canvasY + canvasHeight / 2;

        // Convert angle to radians
        var angleRad = freeForm.Angle * (float)Math.PI / 180f;
        var cos = (float)Math.Cos(angleRad);
        var sin = (float)Math.Sin(angleRad);

        // Calculate the corners of the rotated rectangle
        var corners = new[]
        {
            TransformPoint(canvasX, canvasY, centerX, centerY, cos, sin),
            TransformPoint(canvasX + canvasWidth, canvasY, centerX, centerY, cos, sin),
            TransformPoint(canvasX + canvasWidth, canvasY + canvasHeight, centerX, centerY, cos, sin),
            TransformPoint(canvasX, canvasY + canvasHeight, centerX, centerY, cos, sin)
        };

        var matchingKeys = canvas.Keys.Where(key =>
        {
            ref readonly var rect = ref canvas.GetRectangle(key);

            // Create corners for the key rectangle
            var keyCorners = new[]
            {
                new PointF(rect.Left, rect.Bottom),
                new PointF(rect.Left + rect.Width, rect.Bottom),
                new PointF(rect.Left + rect.Width, rect.Bottom + rect.Height),
                new PointF(rect.Left, rect.Bottom + rect.Height)
            };

            return PolygonContainsPolygon(corners, keyCorners);
        });

        return matchingKeys.ToArray();
    }

    private readonly struct PointF(float x, float y)
    {
        public readonly float X = x;
        public readonly float Y = y;
    }

    private static PointF TransformPoint(float x, float y, float centerX, float centerY, float cos, float sin)
    {
        // Translate point to origin
        var translatedX = x - centerX;
        var translatedY = y - centerY;

        // Rotate
        var rotatedX = translatedX * cos - translatedY * sin;
        var rotatedY = translatedX * sin + translatedY * cos;

        // Translate back
        return new PointF(
            rotatedX + centerX,
            rotatedY + centerY
        );
    }
    
    private static bool PolygonContainsPolygon(PointF[] container, PointF[] contained)
    {
        // First, check if any point of the contained polygon is outside the container
        foreach (var point in contained)
        {
            if (!PointInPolygon(point, container))
            {
                return false;
            }
        }

        return true;
    }

    private static bool PointInPolygon(PointF point, PointF[] polygon)
    {
        bool inside = false;
        int j = polygon.Length - 1;

        for (int i = 0; i < polygon.Length; i++)
        {
            if (((polygon[i].Y > point.Y) != (polygon[j].Y > point.Y)) &&
                (point.X < (polygon[j].X - polygon[i].X) * (point.Y - polygon[i].Y) / 
                    (polygon[j].Y - polygon[i].Y) + polygon[i].X))
            {
                inside = !inside;
            }
            j = i;
        }

        return inside;
    }

    private static bool PolygonsIntersect(PointF[] polygon1, PointF[] polygon2)
    {
        // Use the Separating Axis Theorem (SAT) to detect intersection
        // Check all edges of both polygons as potential separating axes
        
        // Check edges of polygon1
        for (var i = 0; i < polygon1.Length; i++)
        {
            var j = (i + 1) % polygon1.Length;
            var edge = new PointF(
                polygon1[j].X - polygon1[i].X,
                polygon1[j].Y - polygon1[i].Y
            );
            var axis = new PointF(-edge.Y, edge.X); // Normal to the edge
            
            if (HasSeparatingAxis(axis, polygon1, polygon2))
            {
                return false;
            }
        }
        
        // Check edges of polygon2
        for (var i = 0; i < polygon2.Length; i++)
        {
            var j = (i + 1) % polygon2.Length;
            var edge = new PointF(
                polygon2[j].X - polygon2[i].X,
                polygon2[j].Y - polygon2[i].Y
            );
            var axis = new PointF(-edge.Y, edge.X); // Normal to the edge
            
            if (HasSeparatingAxis(axis, polygon1, polygon2))
            {
                return false;
            }
        }
        
        return true;
    }

    private static bool HasSeparatingAxis(PointF axis, PointF[] polygon1, PointF[] polygon2)
    {
        var (min1, max1) = ProjectPolygon(axis, polygon1);
        var (min2, max2) = ProjectPolygon(axis, polygon2);
        
        return max1 < min2 || max2 < min1;
    }

    private static (float Min, float Max) ProjectPolygon(PointF axis, PointF[] polygon)
    {
        var min = float.MaxValue;
        var max = float.MinValue;
        
        foreach (var point in polygon)
        {
            var projection = (point.X * axis.X + point.Y * axis.Y) / 
                             (axis.X * axis.X + axis.Y * axis.Y);
            
            min = Math.Min(min, projection);
            max = Math.Max(max, projection);
        }
        
        return (min, max);
    }

    private void FreeFormObjectOnValuesChanged(object? sender, FreeFormChangedEventArgs e)
    {
        Invalidate();
    }

    private void EffectsOnCanvasChanged(object? sender, EventArgs e)
    {
        Invalidate();
    }

    private void Invalidate()
    {
        _invalidated = true;
    }

    public void Dispose()
    {
        Effects.CanvasChanged -= EffectsOnCanvasChanged;
    }
}