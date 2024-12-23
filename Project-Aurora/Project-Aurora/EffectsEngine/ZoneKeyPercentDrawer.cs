using System;
using System.Drawing;
using System.Linq;
using AuroraRgb.Settings;
using AuroraRgb.Utils;
using Common.Devices;

namespace AuroraRgb.EffectsEngine;

// most of this file is created by AI and adjusted, idk what it does
public class ZoneKeyPercentDrawer(EffectLayer effectLayer)
{
    public void PercentEffect(Color foregroundColor, Color backgroundColor, KeySequence sequence, double value,
        double total = 1.0D, PercentEffectType percentEffectType = PercentEffectType.Progressive,
        double flashPast = 0.0, bool flashReversed = false, bool blinkBackground = false)
    {
        var zoneKeysCache = new ZoneKeysCache();
        zoneKeysCache.SetSequence(sequence);
        var keys = zoneKeysCache.GetKeys();

        if (sequence.Type == KeySequenceType.FreeForm)
        {
            PercentEffectOnFreeForm(foregroundColor, backgroundColor, sequence.Freeform, keys, value, total,
                percentEffectType, flashPast, flashReversed, blinkBackground);
        }
        else
        {
            // Fallback to previous implementation for regular sequences
            PercentEffectOnKeys(foregroundColor, backgroundColor, keys, value, total,
                percentEffectType, flashPast, flashReversed, blinkBackground);
        }
    }

    private void PercentEffectOnFreeForm(Color foregroundColor, Color backgroundColor, FreeFormObject freeform,
        DeviceKeys[] keys, double value, double total, PercentEffectType percentEffectType,
        double flashPast, bool flashReversed, bool blinkBackground)
    {
        switch (percentEffectType)
        {
            case PercentEffectType.AllAtOnce:
            {
                var progressTotal = Math.Clamp(value / total, 0.0, 1.0);

                // Apply flash effect if needed
                if (flashPast > 0.0 && ((flashReversed && progressTotal >= flashPast) || (!flashReversed && progressTotal <= flashPast)))
                {
                    var percent = Math.Sin(Time.GetMillisecondsSinceEpoch() % 1000.0D / 1000.0D * Math.PI);
                    if (blinkBackground)
                        backgroundColor = ColorUtils.BlendColors(backgroundColor, Color.Empty, percent);
                    else
                        foregroundColor = ColorUtils.BlendColors(backgroundColor, foregroundColor, percent);
                }

                var keyColor = ColorUtils.BlendColors(backgroundColor, foregroundColor, progressTotal);

                effectLayer.Set(keys, in keyColor);
            }
                return;
            case PercentEffectType.Progressive:
            case PercentEffectType.Progressive_Gradual:
                PercentEffectOnFreeForm(foregroundColor, backgroundColor, freeform, keys, value, total, flashPast, flashReversed, blinkBackground, effectLayer,
                    percentEffectType == PercentEffectType.Progressive_Gradual);
                return;
        }
    }

    private static void PercentEffectOnFreeForm(Color foregroundColor, Color backgroundColor, FreeFormObject freeform,
        DeviceKeys[] keys, double value, double total,
        double flashPast, bool flashReversed, bool blinkBackground, EffectLayer effectLayer, bool gradual)
    {
        // Calculate progress
        var progressTotal = Math.Clamp(value / total, 0.0, 1.0);

        // Apply flash effect if needed
        if (flashPast > 0.0 && ((flashReversed && progressTotal >= flashPast) || (!flashReversed && progressTotal <= flashPast)))
        {
            var percent = Math.Sin(Time.GetMillisecondsSinceEpoch() % 1000.0D / 1000.0D * Math.PI);
            if (blinkBackground)
                backgroundColor = ColorUtils.BlendColors(backgroundColor, Color.Empty, percent);
            else
                foregroundColor = ColorUtils.BlendColors(backgroundColor, foregroundColor, percent);
        }

        // Calculate freeform object's world coordinates and corners
        var freeformCorners = GetFreeFormCorners(freeform, progressTotal);

        // Iterate through keys and determine their color based on their position
        foreach (var key in keys)
        {
            // Get key's rectangle in canvas coordinates
            ref readonly var keyRect = ref Effects.Canvas.GetRectangle(key);

            // Create key corners
            var keyCorners = new[]
            {
                new PointF(keyRect.Left, keyRect.Bottom),
                new PointF(keyRect.Right, keyRect.Bottom),
                new PointF(keyRect.Right, keyRect.Top),
                new PointF(keyRect.Left, keyRect.Top)
            };

            // Calculate coverage
            var coverageRatio = CalculateKeyCoverage(freeformCorners, keyCorners);

            Color keyColor;
            if (gradual)
            {
                keyColor = ColorUtils.BlendColors(
                    backgroundColor,
                    foregroundColor,
                    Math.Min(coverageRatio, 1.0)
                );
            }
            else
            {
                var isKeyCovered = coverageRatio >= 1f - float.Epsilon;
                keyColor = isKeyCovered ? foregroundColor : backgroundColor;
            }

            effectLayer.Set(key, in keyColor);
        }
    }

    /// <summary>
    /// Calculates the coverage ratio of a key by a freeform object.
    /// </summary>
    /// <param name="freeformCorners">Corners of the freeform object</param>
    /// <param name="keyCorners">Corners of the key</param>
    /// <returns>A ratio between 0 and 1 representing the key's coverage</returns>
    private static float CalculateKeyCoverage(PointF[] freeformCorners, PointF[] keyCorners)
    {
        // Count how many key corners are inside the freeform
        var containedCorners = keyCorners.Count(corner => PointInRectangle(corner, freeformCorners));

        // Calculate coverage based on number of contained corners
        return (float)containedCorners / keyCorners.Length;
    }

    /// <summary>
    /// Gets the corners of a FreeFormObject in canvas coordinates
    /// </summary>
    private static PointF[] GetFreeFormCorners(FreeFormObject freeForm, double progress)
    {
        // Convert FreeForm coordinates to canvas coordinates
        var xPos = (freeForm.X + Effects.Canvas.GridBaselineX) * Effects.Canvas.EditorToCanvasWidth;
        var yPos = (freeForm.Y + Effects.Canvas.GridBaselineY) * Effects.Canvas.EditorToCanvasHeight;
        var width = freeForm.Width * Effects.Canvas.EditorToCanvasWidth * progress;
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

    /// <summary>
    /// Transforms a point by rotating it around a center point
    /// </summary>
    private static PointF TransformPoint(double x, double y, double centerX, double centerY, double cos, double sin)
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

    /// <summary>
    /// Checks if a point is inside a rectangle 
    /// </summary>
    private static bool PointInRectangle(PointF point, PointF[] rectangleCorners)
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

        // Calculate dot products to determine if point is inside
        var dot1 = translatedPoint.X * vector1.X + translatedPoint.Y * vector1.Y;
        var dot2 = translatedPoint.X * vector2.X + translatedPoint.Y * vector2.Y;

        // Check if the point is within the rectangle's side lengths
        return dot1 >= 0 && dot1 <= vector1.X * vector1.X + vector1.Y * vector1.Y &&
               dot2 >= 0 && dot2 <= vector2.X * vector2.X + vector2.Y * vector2.Y;
    }

    private void PercentEffectOnKeys(Color foregroundColor, Color backgroundColor, DeviceKeys[] keys, double value,
        double total, PercentEffectType percentEffectType, double flashPast, bool flashReversed, bool blinkBackground)
    {
        // Previous implementation for non-freeform sequences
        var progressTotal = Math.Clamp(value / total, 0.0, 1.0);
        var progress = progressTotal * keys.Length;

        // Flash effect logic
        if (flashPast > 0.0 && ((flashReversed && progressTotal >= flashPast) || (!flashReversed && progressTotal <= flashPast)))
        {
            var percent = Math.Sin(Time.GetMillisecondsSinceEpoch() % 1000.0D / 1000.0D * Math.PI);
            if (blinkBackground)
                backgroundColor = ColorUtils.BlendColors(backgroundColor, Color.Empty, percent);
            else
                foregroundColor = ColorUtils.BlendColors(backgroundColor, foregroundColor, percent);
        }

        switch (percentEffectType)
        {
            case PercentEffectType.AllAtOnce:
                effectLayer.Set(keys, ColorUtils.BlendColors(in backgroundColor, in foregroundColor, progressTotal));
                break;
            case PercentEffectType.Progressive_Gradual:
                for (var i = 0; i < keys.Length; i++)
                {
                    var currentKey = keys[i];
                    if (i == (int)progress)
                    {
                        var percent = progress - i;
                        var blendColor = ColorUtils.BlendColors(in backgroundColor, in foregroundColor, percent);
                        effectLayer.Set(currentKey, in blendColor);
                    }
                    else if (i < (int)progress)
                        effectLayer.Set(currentKey, in foregroundColor);
                    else
                        effectLayer.Set(currentKey, in backgroundColor);
                }

                break;
            case PercentEffectType.Progressive:
                for (var i = 0; i < keys.Length; i++)
                {
                    var currentKey = keys[i];
                    effectLayer.Set(currentKey, i < (int)progress ? foregroundColor : backgroundColor);
                }

                break;
            case PercentEffectType.Highest_Key:
            {
                effectLayer.Set(keys, in backgroundColor);
                var highestKey = (int)progress;
                effectLayer.Set(keys[highestKey], in foregroundColor);
                break;
            }
            case PercentEffectType.Highest_Key_Blend:
            {
                var highestKey = (int)progress;
                var blendColor = ColorUtils.BlendColors(in backgroundColor, in foregroundColor, progress);
                effectLayer.Set(keys[highestKey], in blendColor);
                break;
            }
        }
    }

    public void PercentEffect(ColorSpectrum spectrum, KeySequence sequence, double value,
        double total, PercentEffectType percentEffectType,
        double flashPast = 0.0, bool flashReversed = false)
    {
        var progressTotal = (value / total) switch
        {
            < 0.0 => 0.0,
            > 1.0 => 1.0,
            _ => value / total
        };
        
        var zoneKeysCache = new ZoneKeysCache();
        zoneKeysCache.SetSequence(sequence);
        var keys = zoneKeysCache.GetKeys();

        var flashAmount = 1.0;
        if (flashPast > 0.0 && ((flashReversed && progressTotal >= flashPast) || (!flashReversed && progressTotal <= flashPast)))
        {
            flashAmount = Math.Sin(Time.GetMillisecondsSinceEpoch() % 1000.0D / 1000.0D * Math.PI);
        }

        switch (percentEffectType, sequence.Type)
        {
            case (PercentEffectType.AllAtOnce, _):
            {
                var color = spectrum.GetColorAt(progressTotal, 1.0f, flashAmount);
                effectLayer.Set(keys, in color);
                return;
            }
            case (PercentEffectType.Progressive, KeySequenceType.Sequence):
            {
                if (progressTotal <= 0)
                {
                    return;
                }
                for (var i = 0; i < keys.Length; i++)
                {
                    var position = (double)i * keys.Length / (keys.Length - 1);
                    var keyProgress = Math.Clamp(position/keys.Length, 0, 1);
                    if (keyProgress > progressTotal)
                    {
                        return;
                    }
                    var key = keys[i];
                    var color = spectrum.GetColorAt(keyProgress, 1.0f, flashAmount);
                    effectLayer.Set(key, in color);
                }
                break;
            }
            case (PercentEffectType.Progressive_Gradual, KeySequenceType.Sequence):
            {
                if (progressTotal <= 0)
                {
                    return;
                }
                var delta = 1.0 / keys.Length;
                for (var i = 0; i < keys.Length; i++)
                {
                    var position = (double)i * keys.Length / (keys.Length - 1);
                    var keyProgress = Math.Clamp(position/keys.Length, 0, 1) * (keys.Length - 1) / keys.Length;
                    var key = keys[i];
                    var color = spectrum.GetColorAt(keyProgress, 1.0f, flashAmount);
                    
                    if (progressTotal - keyProgress < delta)
                    {
                        var keyCoverage = (progressTotal - keyProgress) / delta;
                        if (keyCoverage < float.Epsilon)
                        {
                            return;
                        }
                        var partialColor = ColorUtils.MultiplyColorByScalar(color, keyCoverage);
                        effectLayer.Set(key, in partialColor);
                        return;
                    }
                    effectLayer.Set(key, in color);
                }
                break;
            }
            case (PercentEffectType.Highest_Key, KeySequenceType.Sequence):
            case (PercentEffectType.Highest_Key_Blend, KeySequenceType.Sequence):
            {
                if (progressTotal <= 0)
                {
                    return;
                }
                var highestKey = Math.Clamp((int)(progressTotal * keys.Length), 0, keys.Length - 1);
                
                var key = keys[highestKey];
                var color = spectrum.GetColorAt(progressTotal, 1.0f, flashAmount);
                effectLayer.Set(key, in color);
                return;
            }
            case (_, KeySequenceType.FreeForm):
            {
                PercentEffectOnFreeForm(spectrum, sequence.Freeform, keys, value, total, flashPast, flashReversed, effectLayer);
                return;
            }
        }
    }

    private static void PercentEffectOnFreeForm(ColorSpectrum spectrum, FreeFormObject freeform,
        DeviceKeys[] keys, double value, double total,
        double flashPast, bool flashReversed, EffectLayer effectLayer)
    {
        // Calculate progress
        var progressTotal = Math.Clamp(value / total, 0.0, 1.0);

        var percent = 1.0;
        // Apply flash effect if needed
        if (flashPast > 0.0 && ((flashReversed && progressTotal >= flashPast) || (!flashReversed && progressTotal <= flashPast)))
        {
            percent = Math.Sin(Time.GetMillisecondsSinceEpoch() % 1000.0D / 1000.0D * Math.PI);
        }

        // Calculate freeform object's world coordinates and corners
        var freeformCorners = GetFreeFormCorners(freeform, progressTotal);

        // Iterate through keys and determine their color based on their position
        foreach (var key in keys)
        {
            // Get key's rectangle in canvas coordinates
            ref readonly var keyRect = ref Effects.Canvas.GetRectangle(key);

            // Create key corners
            var keyCorners = new[]
            {
                new PointF(keyRect.Left, keyRect.Bottom),
                new PointF(keyRect.Right, keyRect.Bottom),
                new PointF(keyRect.Right, keyRect.Top),
                new PointF(keyRect.Left, keyRect.Top)
            };

            // Calculate coverage
            var coverageRatio = CalculateKeyCoverage(freeformCorners, keyCorners) * percent;
            if (coverageRatio < double.Epsilon)
            {
                continue;
            }

            var x = GetKeyColorPosition(keyCorners, freeform);

            var keyColor = ColorUtils.MultiplyColorByScalar(
                spectrum.GetColorAt(x),
                Math.Min(coverageRatio, 1.0)
            );

            effectLayer.Set(key, in keyColor);
        }
    }

    private static double GetKeyColorPosition(PointF[] keyCorners, FreeFormObject freeform)
    {
        // First calculate the center point of the key using its corners
        var centerX = keyCorners.Average(p => p.X);
        var centerY = keyCorners.Average(p => p.Y);
        
        // Calculate the key's center position as a ratio of the freeform's width
        // Convert FreeForm coordinates to canvas coordinates
        var xPos = (freeform.X + Effects.Canvas.GridBaselineX) * Effects.Canvas.EditorToCanvasWidth;
        var yPos = (freeform.Y + Effects.Canvas.GridBaselineY) * Effects.Canvas.EditorToCanvasHeight;
        var width = freeform.Width * Effects.Canvas.EditorToCanvasWidth;
        var height = freeform.Height * Effects.Canvas.EditorToCanvasHeight;
        
        var x = (centerX - xPos) / width;
        var y = (centerY - yPos) / height;
        
        var sin = Math.Sin(freeform.Angle * Math.PI / 180);
        var cos = Math.Cos(freeform.Angle * Math.PI / 180);

        var keyColorPosition = x * cos - y * sin;
        if (keyColorPosition < 0)
        {
            return 1 + keyColorPosition;
        }
        return keyColorPosition;
    }
}