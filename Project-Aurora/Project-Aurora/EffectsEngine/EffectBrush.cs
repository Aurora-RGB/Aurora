using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Windows.Media;
using AuroraRgb.Utils;
using AuroraRgb.Utils.Json;
using Newtonsoft.Json;
using Brush = System.Drawing.Brush;
using Color = System.Drawing.Color;
using LinearGradientBrush = System.Drawing.Drawing2D.LinearGradientBrush;
using Point = System.Windows.Point;

namespace AuroraRgb.EffectsEngine;

public sealed class EffectBrush : IDisposable
{
    public enum BrushType
    {
        None,
        Solid,
        Linear,
        Radial
    }

    public enum BrushWrap
    {
        None,
        Repeat,
        Reflect
    }

    [JsonProperty("type")]
    public BrushType Type { get; } = BrushType.None;

    [JsonProperty("wrap")]
    public BrushWrap Wrap { get; } = BrushWrap.None;

    [JsonProperty("color_gradients")]
    [JsonConverter(typeof(SortedDictionaryAdapter))]
    public IReadOnlyDictionary<double, Color> ColorGradients { get; init; } = new SortedDictionary<double, Color>();

    [JsonProperty("start")]
    public PointF Start { get; set; }

    [JsonProperty("end")]
    public PointF End { get; set; }

    [JsonProperty("center")]
    public PointF Center { get; set; }

    private Brush? _drawingBrush;
    private System.Windows.Media.Brush? _mediaBrush;

    [JsonConstructor]
    public EffectBrush(BrushType type, BrushWrap wrap, SortedDictionary<double, Color> colorGradients,
        PointF start, PointF end, PointF center, Brush drawingBrush, System.Windows.Media.Brush mediaBrush)
    {
        Type = type;
        Wrap = wrap;
        if (colorGradients.Any(kv => kv.Key > 1))
        {
            colorGradients.Clear();
        }
        if (!colorGradients.ContainsKey(0.0))
        {
            colorGradients[0] = Color.Transparent;
        }
        if (!colorGradients.ContainsKey(1.0))
        {
            colorGradients[1] = Color.Transparent;
        }
        ColorGradients = colorGradients;
        Start = start;
        End = end;
        Center = center;
        _drawingBrush = drawingBrush;
        _mediaBrush = mediaBrush;
    }

    public EffectBrush()
    {
        Type = BrushType.Solid;

        ColorGradients = new SortedDictionary<double, Color>
        {
            {0, Color.Red},
            {1, Color.Blue},
        };

        Start = new PointF(0, 0);
        End = new PointF(1, 0);
        Center = new PointF(0.0f, 0.0f);
    }

    public EffectBrush(BrushType brushType, BrushWrap wrap = BrushWrap.None)
    {
        Type = brushType;
        Wrap = wrap;

        ColorGradients = new SortedDictionary<double, Color>
        {
            {0, Color.Red},
            {1, Color.Blue},
        };

        Start = new PointF(0, 0);
        End = new PointF(1, 0);
        Center = new PointF(0.0f, 0.0f);
    }

    public EffectBrush(EffectBrush otherBrush)
    {
        Type = otherBrush.Type;
        Wrap = otherBrush.Wrap;
        ColorGradients = otherBrush.ColorGradients.ToImmutableSortedDictionary();
        Start = otherBrush.Start;
        End = otherBrush.End;
        Center = otherBrush.Center;
    }

    public EffectBrush(ColorSpectrum spectrum, BrushType brushType = BrushType.Linear, BrushWrap wrap = BrushWrap.None)
    {
        Type = brushType;
        Wrap = wrap;

        ColorGradients = spectrum.GetSpectrumColors().ToImmutableSortedDictionary();

        Start = new PointF(0, 0);
        End = new PointF(1, 0);
        Center = new PointF(0.0f, 0.0f);
    }

    public EffectBrush(Brush brush)
    {
        switch (brush)
        {
            case SolidBrush:
                Type = BrushType.Solid;

                ColorGradients = new SortedDictionary<double, Color>
                {
                    {0, Color.Red},
                    {1, Color.Blue},
                };

                Wrap = BrushWrap.Repeat;
                break;
            case LinearGradientBrush lgb:
                Type = BrushType.Linear;

                Start = lgb.Rectangle.Location;
                End = new PointF(lgb.Rectangle.Width, lgb.Rectangle.Height);
                Center = new PointF(0.0f, 0.0f);

                Wrap = lgb.WrapMode switch
                {
                    WrapMode.Clamp => BrushWrap.None,
                    WrapMode.Tile => BrushWrap.Repeat,
                    WrapMode.TileFlipXY => BrushWrap.Reflect,
                    _ => Wrap
                };

                try
                {
                    if (lgb.InterpolationColors != null && lgb.InterpolationColors.Colors.Length == lgb.InterpolationColors.Positions.Length)
                    {
                        ColorGradients = Enumerable.Range(0, lgb.InterpolationColors.Colors.Length)
                            .Where(i => lgb.InterpolationColors.Positions[i] >= 0.0f && lgb.InterpolationColors.Positions[i] <= 1.0f)
                            .OrderBy(i => lgb.InterpolationColors.Positions[i])
                            .ToImmutableSortedDictionary(i => (double)lgb.InterpolationColors.Positions[i], i => lgb.InterpolationColors.Colors[i]);
                    }
                }
                catch (Exception)
                {
                    ColorGradients = Enumerable.Range(0, lgb.LinearColors.Length)
                        .ToImmutableSortedDictionary(i => i / (double)(lgb.LinearColors.Length - 1), i => lgb.LinearColors[i]);
                }

                break;
            case PathGradientBrush pgb:
                Type = BrushType.Radial;

                Start = pgb.Rectangle.Location;
                End = new PointF(pgb.Rectangle.Width, pgb.Rectangle.Height);
                Center = new PointF(
                    pgb.CenterPoint.X,
                    pgb.CenterPoint.Y
                );

                Wrap = pgb.WrapMode switch
                {
                    WrapMode.Clamp => BrushWrap.None,
                    WrapMode.Tile => BrushWrap.Repeat,
                    WrapMode.TileFlipXY => BrushWrap.Reflect,
                    _ => Wrap
                };

                try
                {
                    if (pgb.InterpolationColors != null && pgb.InterpolationColors.Colors.Length == pgb.InterpolationColors.Positions.Length)
                    {
                        ColorGradients = Enumerable.Range(0, pgb.InterpolationColors.Colors.Length)
                            .Where(i => pgb.InterpolationColors.Positions[i] >= 0.0f && pgb.InterpolationColors.Positions[i] <= 1.0f)
                            .OrderBy(i => pgb.InterpolationColors.Positions[i])
                            .ToImmutableSortedDictionary(i => (double)pgb.InterpolationColors.Positions[i], i => pgb.InterpolationColors.Colors[i]);
                    }
                }
                catch (Exception)
                {
                    ColorGradients = Enumerable.Range(0, pgb.SurroundColors.Length)
                        .ToImmutableSortedDictionary(i => i / (double)(pgb.SurroundColors.Length - 1), i => pgb.SurroundColors[i]);
                }

                break;
        }

        if (ColorGradients.Count == 0)
        {
            ColorGradients = new SortedDictionary<double, Color>
            {
                { 0, Color.Transparent },
                { 1, Color.Transparent },
            };
            return;
        }
        
        var addFirstColor = !ColorGradients.ContainsKey(0.0f);
        var addLastColor = !ColorGradients.ContainsKey(1.0f);
        
        if (addFirstColor || addLastColor)
        {
            var dict = new Dictionary<double, Color>(ColorGradients);
            if (addFirstColor)
                dict[0.0] = ColorGradients.FirstOrDefault().Value;
            if (addLastColor)
                dict[1.0] = ColorGradients.LastOrDefault().Value;
            ColorGradients = dict.ToImmutableSortedDictionary();
        }
    }

    public EffectBrush(System.Windows.Media.Brush brush)
    {
        switch (brush)
        {
            case SolidColorBrush colorBrush:
                Type = BrushType.Solid;

                Wrap = BrushWrap.Repeat;

                ColorGradients = new SortedDictionary<double, Color>
                {
                    {0, ColorUtils.MediaColorToDrawingColor(colorBrush.Color)},
                    {1, ColorUtils.MediaColorToDrawingColor(colorBrush.Color)},
                };
                break;
            case System.Windows.Media.LinearGradientBrush lgb:
                Type = BrushType.Linear;

                Start = new PointF((float)lgb.StartPoint.X, (float)lgb.StartPoint.Y);
                End = new PointF((float)lgb.EndPoint.X, (float)lgb.EndPoint.Y);
                Center = new PointF(0.0f, 0.0f);

                Wrap = lgb.SpreadMethod switch
                {
                    GradientSpreadMethod.Pad => BrushWrap.None,
                    GradientSpreadMethod.Repeat => BrushWrap.Repeat,
                    GradientSpreadMethod.Reflect => BrushWrap.Reflect,
                    _ => Wrap
                };

                ColorGradients = lgb.GradientStops
                    .Where(stop => (float)stop.Offset >= 0.0f && (float)stop.Offset <= 1.0f)
                    .DistinctBy(e => e.Offset)
                    .OrderBy(e => e.Offset)
                    .ToImmutableSortedDictionary(stop => stop.Offset, stop => ColorUtils.MediaColorToDrawingColor(stop.Color));
                break;
            case RadialGradientBrush rgb:
                Type = BrushType.Radial;

                Start = new PointF(0, 0);
                End = new PointF((float)rgb.RadiusX * 2.0f, (float)rgb.RadiusY * 2.0f);
                Center = new PointF(
                    (float)rgb.Center.X,
                    (float)rgb.Center.Y
                );

                Wrap = rgb.SpreadMethod switch
                {
                    GradientSpreadMethod.Pad => BrushWrap.None,
                    GradientSpreadMethod.Repeat => BrushWrap.Repeat,
                    GradientSpreadMethod.Reflect => BrushWrap.Reflect,
                    _ => Wrap
                };

                ColorGradients = rgb.GradientStops
                    .Where(stop => (float)stop.Offset >= 0.0f && (float)stop.Offset <= 1.0f)
                    .OrderBy(e => e.Offset)
                    .ToImmutableSortedDictionary(stop => stop.Offset, stop => ColorUtils.MediaColorToDrawingColor(stop.Color));
                break;
        }

        if (ColorGradients.Count == 0)
        {
            ColorGradients = new SortedDictionary<double, Color>
            {
                { 0, Color.Transparent },
                { 1, Color.Transparent },
            };
            return;
        }
        
        var addFirstColor = !ColorGradients.ContainsKey(0.0f);
        var addLastColor = !ColorGradients.ContainsKey(1.0f);
        
        if (addFirstColor || addLastColor)
        {
            var dict = new Dictionary<double, Color>(ColorGradients);
            if (addFirstColor)
                dict[0.0] = ColorGradients.FirstOrDefault().Value;
            if (addLastColor)
                dict[1.0] = ColorGradients.LastOrDefault().Value;
            ColorGradients = dict.ToImmutableSortedDictionary();
        }
    }

    public Brush GetDrawingBrush()
    {
        _drawingBrush ??= Type switch
        {
            BrushType.Solid => new SolidBrush(ColorGradients[0.0f]),
            BrushType.Linear => GetLinearBrush(),
            BrushType.Radial => GetRadialBrush(),
            _ => new SolidBrush(Color.Transparent)
        };

        return _drawingBrush;
    }

    private LinearGradientBrush GetLinearBrush()
    {
        return BrushUtils.GetLinearBrush(ColorGradients, Start, End, Wrap);
    }

    private PathGradientBrush GetRadialBrush()
    {
        var gPath = new GraphicsPath();
        gPath.AddEllipse(
            new RectangleF(
                Start.X,
                Start.Y,
                End.X,
                End.Y
            ));

        var brush = new PathGradientBrush(
            gPath
        );

        brush.WrapMode = Wrap switch
        {
            BrushWrap.Repeat => WrapMode.Tile,
            BrushWrap.Reflect => WrapMode.TileFlipXY,
            _ => brush.WrapMode
        };

        var brushColors = new List<Color>();
        var brushPositions = new List<float>();

        foreach (var kvp in ColorGradients)
        {
            brushPositions.Add(1.0f - (float)kvp.Key);
            brushColors.Add(kvp.Value);
        }

        brush.CenterPoint = Center;

        brushColors.Reverse();
        brushPositions.Reverse();

        var colorBlend = new ColorBlend
        {
            Colors = brushColors.ToArray(),
            Positions = brushPositions.ToArray()
        };
        brush.InterpolationColors = colorBlend;

        return brush;
    }

    public System.Windows.Media.Brush GetMediaBrush()
    {
        if (_mediaBrush != null) return _mediaBrush;
        switch (Type)
        {
            case BrushType.Solid:
            {
                var brush = new SolidColorBrush(
                    ColorUtils.DrawingColorToMediaColor(ColorGradients[0.0f])
                );
                brush.Freeze();

                _mediaBrush = brush;
                break;
            }
            case BrushType.Linear:
            {
                var collection = new GradientStopCollection();

                foreach (var kvp in ColorGradients)
                {
                    collection.Add(
                        new GradientStop(
                            ColorUtils.DrawingColorToMediaColor(kvp.Value),
                            kvp.Key)
                    );
                }

                var brush = new System.Windows.Media.LinearGradientBrush(collection)
                {
                    StartPoint = new Point(Start.X, Start.Y),
                    EndPoint = new Point(End.X, End.Y)
                };

                brush.SpreadMethod = Wrap switch
                {
                    BrushWrap.None => GradientSpreadMethod.Pad,
                    BrushWrap.Repeat => GradientSpreadMethod.Repeat,
                    BrushWrap.Reflect => GradientSpreadMethod.Reflect,
                    _ => brush.SpreadMethod
                };

                _mediaBrush = brush;
                break;
            }
            case BrushType.Radial:
            {
                var collection = new GradientStopCollection();

                foreach (var kvp in ColorGradients)
                {
                    collection.Add(
                        new GradientStop(
                            ColorUtils.DrawingColorToMediaColor(kvp.Value),
                            kvp.Key)
                    );
                }

                var brush = new RadialGradientBrush(collection)
                {
                    Center = new Point(Center.X, Center.Y),
                    RadiusX = End.X / 2.0,
                    RadiusY = End.Y / 2.0
                };

                brush.SpreadMethod = Wrap switch
                {
                    BrushWrap.None => GradientSpreadMethod.Pad,
                    BrushWrap.Repeat => GradientSpreadMethod.Repeat,
                    BrushWrap.Reflect => GradientSpreadMethod.Reflect,
                    _ => brush.SpreadMethod
                };

                _mediaBrush = brush;
                break;
            }
            default:
            {
                var brush = new SolidColorBrush(
                    System.Windows.Media.Color.FromArgb(255, 255, 0, 0)
                );
                brush.Freeze();

                _mediaBrush = brush;
                break;
            }
        }

        return _mediaBrush;
    }

    public ColorSpectrum GetColorSpectrum()
    {
        var spectrum = new ColorSpectrum();

        if(Type == BrushType.Solid)
        {
            spectrum = new ColorSpectrum(ColorGradients[0.0f]);
        }
        else
        {
            foreach (var color in ColorGradients)
                spectrum.SetColorAt(color.Key, color.Value);
        }

        return spectrum;
    }

    /// <summary>
    /// Blends two EffectBrushes together by a specified amount
    /// </summary>
    /// <param name="otherBrush">The foreground EffectBrush (When percent is at 1.0D, only this EffectBrush is shown)</param>
    /// <param name="percent">The blending percent value</param>
    /// <returns>The blended EffectBrush</returns>
    public EffectBrush BlendEffectBrush(EffectBrush otherBrush, double percent)
    {
        if (percent <= 0.0)
            return new EffectBrush(this);
        if (percent >= 1.0)
            return new EffectBrush(otherBrush);

        var currentSpectrum = new ColorSpectrum(GetColorSpectrum());
        var newSpectrum = new ColorSpectrum(currentSpectrum).MultiplyByScalar(1.0 - percent);

        foreach (var (key, value) in otherBrush.ColorGradients)
        {
            var bgColor = currentSpectrum.GetColorAt(key);

            newSpectrum.SetColorAt(key, ColorUtils.BlendColors(bgColor, value, percent));
        }

        var returnBrush = new EffectBrush(newSpectrum, Type, Wrap)
        {
            Start = new PointF((float)(Start.X * (1.0 - percent) + otherBrush.Start.X * percent), (float)(Start.Y * (1.0 - percent) + otherBrush.Start.Y * percent)),
            End = new PointF((float)(End.X * (1.0 - percent) + otherBrush.End.X * percent), (float)(End.Y * (1.0 - percent) + otherBrush.End.Y * percent)),
            Center = new PointF((float)(Center.X * (1.0 - percent) + otherBrush.Center.X * percent), (float)(Center.Y * (1.0 - percent) + otherBrush.Center.Y * percent))
        };

        return returnBrush;
    }

    public override bool Equals(object? obj)
    {
        if (ReferenceEquals(null, obj)) return false;
        if (ReferenceEquals(this, obj)) return true;
        if (obj.GetType() != GetType()) return false;
        return Equals((EffectBrush)obj);
    }

    public bool Equals(EffectBrush p)
    {
        if (ReferenceEquals(null, p)) return false;
        if (ReferenceEquals(this, p)) return true;

        return Type == p.Type &&
               Wrap == p.Wrap &&
               ColorGradients.Equals(p.ColorGradients) &&
               Start.Equals(p.Start) &&
               End.Equals(p.End) &&
               Center.Equals(p.Center);
    }

    public override int GetHashCode()
    {
        unchecked
        {
            var hash = 17;
            hash = hash * 23 + Type.GetHashCode();
            hash = hash * 23 + Wrap.GetHashCode();
            hash = hash * 23 + ColorGradients.GetHashCode();
            hash = hash * 23 + Start.GetHashCode();
            hash = hash * 23 + End.GetHashCode();
            hash = hash * 23 + Center.GetHashCode();
            return hash;
        }
    }

    public void Dispose()
    {
        _drawingBrush?.Dispose();
    }
}