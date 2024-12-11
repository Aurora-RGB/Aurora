using System;
using System.Drawing;
using System.Linq;
using AuroraRgb.Settings;
using AuroraRgb.Utils;
using Common.Devices;

namespace AuroraRgb.EffectsEngine;

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
}