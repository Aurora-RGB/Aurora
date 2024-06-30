using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using AuroraRgb.Utils;

namespace AuroraRgb.EffectsEngine;

/// <summary>
/// A factory that can create a segmented radial brush.
/// </summary>
/// <remarks>
/// I originally tried creating this effect using the <see cref="PathGradientBrush"/>, however I cannot find a way of removing the central colour. This means that the
/// colours gradually fade to another colour in the centre. Since the points on the path would need to be equidistant from the centre to preserve the angle and gradients,
/// it means that some of the brush is cut off and the colours appear washed out. All round, not ideal for this use case, so that is the reason I have created this instead.
/// </remarks>
public class SegmentedRadialBrushFactory : ICloneable {
    // The resolution of the base texture size.
    private const int TextureSize = 200;
    private static readonly Rectangle RenderArea = new(0, 0, TextureSize, TextureSize);
    private static readonly SolidBrush Fallback = new(Color.Transparent);

    private ColorStopCollection _colors;
    private readonly int _segmentCount = 24;
    private TextureBrush _baseBrush;

    public SegmentedRadialBrushFactory(ColorStopCollection colors) {
        _colors = colors;
        _baseBrush = CreateBaseTextureBrush();
    }

    /// <summary>
    /// Gets or sets the colors and their orders in use by the brush.
    /// </summary>
    public ColorStopCollection Colors {
        get => _colors;
        set {
            // If the colors are equal, don't do anything
            if (_colors.StopsEqual(value))
                return;

            // If they are not equal, create a new texture brush
            _colors = value;
            var oldBrush = _baseBrush;
            _baseBrush = CreateBaseTextureBrush();
            oldBrush.Dispose();
        }
    }

    /// <summary>
    /// How many segments should be created for this brush. Larger values appear smoother by may run more slowly.
    /// </summary>
    public int SegmentCount {
        get => _segmentCount;
        init {
            if (_segmentCount <= 0)
                throw new ArgumentOutOfRangeException(nameof(SegmentCount), "Segment count must not be lower than 1.");
            if (_segmentCount == value) return;
            _segmentCount = value;
            var oldBrush = _baseBrush;
            _baseBrush = CreateBaseTextureBrush();
            oldBrush.Dispose();
        }
    }

    /// <summary>
    /// Creates a new base brush from the current properties.
    /// </summary>
    private TextureBrush CreateBaseTextureBrush() {
        var angle = 360f / _segmentCount;
        var segmentOffset = 1f / _segmentCount; // how much each segment moves the offset forwards on the gradient

        // Get a list of all stops in the stop collection.
        // We use this to optimise the interpolation of the colors.
        // If we were to use ColorStopCollection.GetColorAt, it may end up running numerous for loops over the same stops, but given
        // the special requirements here, we can eliminate that and use less for loops and make the ones we do use slightly more optimal.
        var stops = _colors.ToList();
        var currentOffset = segmentOffset / 2;
        var stopIdx = 0;

        // If there isn't a stop at offsets 0 and 1, create them. This makes it easier during the loop since we don't have to check if we're left/right of the first/last stops.
        if (stops[0].Key != 0)
            stops.Insert(0, new KeyValuePair<float, Color>(0f, stops[0].Value));
        if (stops[stops.Count - 1].Key != 1)
            stops.Add(new KeyValuePair<float, Color>(1f, stops[^1].Value));

        // Create and draw texture
        var texture = new Bitmap(TextureSize, TextureSize);
        using (var gfx = Graphics.FromImage(texture)) {
            var solidBrush = new SolidBrush(Color.Transparent);
            for (var i = 0; i < _segmentCount; i++) {

                // Move the stop index forwards if required.
                //  - It needs to more fowards until the the stop at that index is to the left of the current offset and the point at that index+1 is to the right.
                //  - If it is exactly on a stop, make that matched stop at that index.
                while (stops[stopIdx + 1].Key < currentOffset)
                    stopIdx++;

                // Now that stopIdx is in the right place, we can figure out which color we need.
                var color = stops[stopIdx].Key == currentOffset
                    ? stops[stopIdx].Value // if exactly on a stop, don't need to interpolate it
                    : ColorUtils.BlendColors( // otherwise, we need to calculate the blend between the two stops
                        stops[stopIdx].Value,
                        stops[stopIdx + 1].Value,
                        (currentOffset - stops[stopIdx].Key) / (stops[stopIdx + 1].Key - stops[stopIdx].Key)
                    );

                // Draw this segment
                solidBrush.Color = color;
                gfx.FillPie(solidBrush, RenderArea, i * angle, angle);

                // Bump the offset
                currentOffset += segmentOffset;
            }
            solidBrush.Dispose();
        }

        // Create the texture brush from our custom bitmap texture
        return new TextureBrush(texture);
    }

    /// <summary>
    /// Gets the brush that will be centered on and sized for the specified region.
    /// </summary>
    /// <param name="region">The region which defines where the brush will be drawn and where the brush will be centered.</param>
    /// <param name="angle">The angle which the brush will be rendered at.</param>
    /// <param name="keepAspectRatio">If <c>true</c>, the scale transformation will have the same value in x as it does in y. If <c>false</c>, the scale in each dimension may be different.
    /// When <c>true</c>, the sizes/areas of each color may appear different (due to being cut off), however when <c>false</c>, they appear more consistent.
    /// If the brush is animated, <c>true</c> will make the speeed appear constant whereas <c>false</c> will cause the rotation to appear slower on the shorter side.</param>
    public Brush GetBrush(RectangleF region, float angle = 0, bool keepAspectRatio = true) {
        // Check if the region has a 0 size. If so, just return a blank brush instead (the matrix becomes invalid with 0 size scaling).
        if (region.Width <= 0.01 || region.Height <= 0.01) return Fallback;

        var mtx = new Matrix();

        // Translate it so that the center of the texture (where all the colors meet) is at 0,0
        mtx.Translate(-TextureSize / 2f, -TextureSize / 2f, MatrixOrder.Append);

        // Then, rotate it to the target angle
        mtx.Rotate(angle, MatrixOrder.Append);

        // Scale it so that it'll still completely cover the textureSize area.
        // 1.45 is a rough approximation of SQRT(2) [it's actually 1.414 but we want to allow a bit of space incase of artifacts at the edges]
        mtx.Scale(1.45f, 1.45f, MatrixOrder.Append);

        // Next we need to scale the texture so that it'll cover the area defined by the region
        float sx = region.Width / TextureSize, sy = region.Height / TextureSize;
        // If the aspect ratio is locked, we want to scale both dimensions up to the biggest required scale
        if (keepAspectRatio)
            sx = sy = Math.Max(sx, sy);
        mtx.Scale(sx, sy, MatrixOrder.Append);

        // Finally, we need to translate the texture so that it is in the center of the region
        // (At this point, the center of the texture where the colors meet is still at 0,0)
        mtx.Translate(region.Left + (region.Width / 2), region.Top + (region.Height / 2), MatrixOrder.Append);

        // Apply the transformation and return the texture brush
        _baseBrush.Transform = mtx;
        return _baseBrush;
    }

    /// <summary>
    /// Creates a clone of this factory.
    /// </summary>
    public object Clone() => new SegmentedRadialBrushFactory(new ColorStopCollection(_colors)) { SegmentCount = SegmentCount };
}