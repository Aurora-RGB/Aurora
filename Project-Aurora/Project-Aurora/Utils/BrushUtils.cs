using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing.Drawing2D;
using System.Globalization;
using System.Linq;
using System.Windows.Data;
using AuroraRgb.EffectsEngine;
using D = System.Drawing;
using M = System.Windows.Media;

namespace AuroraRgb.Utils
{
    public static class BrushUtils
    {
        public static LinearGradientBrush GetLinearBrush(IReadOnlyDictionary<double, D.Color> colorGradients, D.PointF start, D.PointF end, EffectBrush.BrushWrap wrap = EffectBrush.BrushWrap.None)
        {
            var brushColors = new List<D.Color>();
            var brushPositions = new List<float>();

            foreach (var kvp in colorGradients)
            {
                brushPositions.Add((float)kvp.Key);
                brushColors.Add(kvp.Value);
            }

            var colorBlend = new ColorBlend
            {
                Colors = brushColors.ToArray(),
                Positions = brushPositions.ToArray()
            };
            var brush = new LinearGradientBrush(
                start,
                end,
                D.Color.Red,
                D.Color.Red
            );
            brush.InterpolationColors = colorBlend;

            brush.WrapMode = wrap switch
            {
                EffectBrush.BrushWrap.Repeat => WrapMode.Tile,
                EffectBrush.BrushWrap.Reflect => WrapMode.TileFlipXY,
                _ => brush.WrapMode
            };

            return brush;
        }
    }

    /// <summary>
    /// A collection which stores and interpolates a collection of colors that can represent a gradient.
    /// </summary>
    /// <remarks>
    /// I've made this as it's own class rather than using one of the built-in collections as there can sometimes be UI multi-thead issues if trying
    /// to access a gradient stop collection that is being used by a gradient editor in the UI.
    /// </remarks>
    public class ColorStopCollection : IEnumerable<KeyValuePair<float, D.Color>> {

        private readonly SortedList<float, D.Color> _stops = new();

        /// <summary>
        /// Creates an empty ColorStopCollection.
        /// </summary>
        public ColorStopCollection() { }

        /// <summary>
        /// Creates a ColorStopCollection from the given float-color key-value-pairs.
        /// </summary>
        public ColorStopCollection(IEnumerable<KeyValuePair<float, D.Color>> stops) {
            foreach (var stop in stops)
                SetColorAt(stop.Key, stop.Value);
        }

        /// <summary>
        /// Creates a ColorStopCollection from the given colors, which are automatically evenly placed, with the first being at offset 0 and the last at offset 1.
        /// </summary>
        public ColorStopCollection(IEnumerable<D.Color> colors) {
            var count = colors.Count();
            if (count > 0) {
                float offset = 0, d = count > 2 ? 1f / (count - 1f) : 0f;
                foreach (var color in colors) {
                    SetColorAt(offset, color);
                    offset += d;
                }
            }
        }

        /// <summary>
        /// Gets or sets the color at the specified offset.
        /// When setting a value, a new color stop will be created at the given offset if one does not already exist.
        /// </summary>
        /// <param name="offset"></param>
        /// <returns></returns>
        public D.Color this[float offset] {
            get => GetColorAt(offset);
            set => SetColorAt(offset, value);
        }

        /// <summary>
        /// Gets the color at the specific offset.
        /// If this point is not at a stop, it's value is interpolated.
        /// </summary>
        public D.Color GetColorAt(float offset) {
            // If there are no stops, return a transparent color
            if (_stops.Count == 0)
                return D.Color.Transparent;

            offset = Math.Max(Math.Min(offset, 1), 0);

            // First, check if the target offset is at a stop. If so, return the value of that stop.
            if (_stops.ContainsKey(offset))
                return _stops[offset];
            
            // Next, check to see if the target offset is before the first stop or after the last, if so, return that stop.
            if (offset < _stops.Keys[0])
                return _stops.First().Value;
            if (offset > _stops.Keys[_stops.Keys.Count - 1])
                return _stops.Last().Value;

            // At this point, offset is determined to be between two stops, so find which two and then interpolate them.
            for (var i = 1; i < _stops.Count; i++) {
                if (offset > _stops.Keys[i - 1] && offset < _stops.Keys[i])
                    return ColorUtils.BlendColors(
                        _stops.Values[i - 1],
                        _stops.Values[i],
                        (offset - _stops.Keys[i - 1]) / (_stops.Keys[i] - _stops.Keys[i - 1])
                    );
            }

            // Logically, should never get here.
            throw new InvalidOperationException("No idea what happened.");
        }

        /// <summary>
        /// Sets the color at the specified offset to the given value.
        /// If an offset does not exist at this point, one will be created.
        /// </summary>
        private void SetColorAt(float offset, D.Color color) {
            if (offset is < 0 or > 1)
                throw new ArgumentOutOfRangeException(nameof(offset), $"Gradient stop at offset {offset} is out of range. Value must be between 0 and 1 (inclusive).");
            _stops[offset] = color;
        }

        /// <summary>
        /// Creates a new media brush from this stop collection.
        /// </summary>
        public M.LinearGradientBrush ToMediaBrush() {
            M.GradientStopCollection gsc;
            if (_stops.Count == 0)
                gsc = new M.GradientStopCollection(new[] { new M.GradientStop(M.Colors.Transparent, 0), new M.GradientStop(M.Colors.Transparent, 1) });
            else if (_stops.Count == 1)
                gsc = new M.GradientStopCollection(new[] { new M.GradientStop(_stops.Values[0].ToMediaColor(), 0), new M.GradientStop(_stops.Values[0].ToMediaColor(), 1) });
            else
                gsc = new M.GradientStopCollection(_stops.Select(s => new M.GradientStop(s.Value.ToMediaColor(), s.Key)));
            return new M.LinearGradientBrush(gsc);
        }

        /// <summary>
        /// Creates a new stop collection from the given media brush.
        /// </summary>
        public static ColorStopCollection FromMediaBrush(M.Brush brush)
        {
            switch (brush)
            {
                case M.GradientBrush gb:
                {
                    var colorPositions = gb.GradientStops
                        .GroupBy(gs => gs.Offset)
                        .Distinct()
                        .ToDictionary(
                            gs => (float)gs.First().Offset,
                            gs => gs.First().Color.ToDrawingColor()
                        );
                    return new ColorStopCollection(colorPositions);
                }
                case M.SolidColorBrush sb:
                    return new ColorStopCollection { { 0f, sb.Color.ToDrawingColor() } };
                default:
                    throw new InvalidOperationException($"Brush of type '{brush.GetType().Name} could not be converted to a ColorStopCollection.");
            }
        }

        /// <summary>
        /// Determines if this color stop collection contains the same stops as another collection.
        /// </summary>
        public bool StopsEqual(ColorStopCollection other) => Enumerable.SequenceEqual(_stops, other._stops);

        #region IEnumerable
        /// <summary>Alias for <see cref="SetColorAt(float, D.Color)"/> to allow for list constructor syntax.</summary>
        public void Add(float offset, D.Color color) => SetColorAt(offset, color);

        public IEnumerator<KeyValuePair<float, D.Color>> GetEnumerator() => ((IEnumerable<KeyValuePair<float, D.Color>>)_stops).GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable<KeyValuePair<float, D.Color>>)_stops).GetEnumerator();
        #endregion
    }

    /// <summary>
    /// Converter that converts a <see cref="M.Color"/> into a <see cref="M.SolidColorBrush"/>.
    /// Does not support converting back.
    /// </summary>
    public class ColorToBrushConverter : IValueConverter {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) => new M.SolidColorBrush((value as M.Color?) ?? M.Color.FromArgb(0, 0, 0, 0));
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => throw new NotImplementedException();
    }
}
