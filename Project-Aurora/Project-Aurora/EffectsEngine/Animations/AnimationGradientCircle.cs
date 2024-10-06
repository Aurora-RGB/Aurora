using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using AuroraRgb.Bitmaps;
using Newtonsoft.Json;

namespace AuroraRgb.EffectsEngine.Animations;

public class AnimationGradientCircle : AnimationCircle
{
    public EffectBrush GradientBrush { get; private set; }

    [JsonConstructor]
    public AnimationGradientCircle(EffectBrush gradientBrush)
    {
        GradientBrush = gradientBrush;
    }

    public AnimationGradientCircle(RectangleF dimension, EffectBrush brush, int width = 1, float duration = 0.0f) : base(dimension, Color.Transparent, width, duration)
    {
        GradientBrush = new EffectBrush(brush)
        {
            Start = new PointF(0.0f, 0.0f),
            End = new PointF(1.0f, 1.0f),
            Center = new PointF(0.5f, 0.5f)
        };
    }

    public AnimationGradientCircle(PointF center, float radius, EffectBrush brush, int width = 1, float duration = 0.0f) : base(center, radius, Color.Transparent, width, duration)
    {
        GradientBrush = new EffectBrush(brush)
        {
            Start = new PointF(0.0f, 0.0f),
            End = new PointF(1.0f, 1.0f),
            Center = new PointF(0.5f, 0.5f)
        };
    }

    public AnimationGradientCircle(float x, float y, float radius, EffectBrush brush, int width = 1, float duration = 0.0f) : base(x, y, radius, Color.Transparent, width, duration)
    {
        GradientBrush = new EffectBrush(brush)
        {
            Start = new PointF(0.0f, 0.0f),
            End = new PointF(1.0f, 1.0f),
            Center = new PointF(0.5f, 0.5f)
        };
    }

    protected override void VirtUpdate()
    {
        base.VirtUpdate();

        var newColorGradients = new SortedDictionary<double, Color>();
        var spectrum = GradientBrush.GetColorSpectrum();

        if (_radius == 0)
            return;
        
        var cutOffPoint = _width / _radius;
        if (cutOffPoint < 1.0f)
        {
            cutOffPoint = 1.0f - cutOffPoint;

            foreach (var kvp in spectrum.GetSpectrumColors())
                newColorGradients.Add((1 - cutOffPoint) * kvp.Key + cutOffPoint, kvp.Value);

            newColorGradients.Add(cutOffPoint - 0.0001f, Color.Transparent);
            newColorGradients.Add(0.0f, Color.Transparent);

            //TODO this is very inefficient
            GradientBrush = new EffectBrush
            {
                ColorGradients = newColorGradients,
            };
        }
        else if (cutOffPoint > 1.0f)
        {
            foreach (var kvp in spectrum.GetSpectrumColors())
            {
                if (kvp.Key >= 1 - 1 / cutOffPoint)
                {
                    newColorGradients.Add((1 - 1 / cutOffPoint) * kvp.Key + cutOffPoint, kvp.Value);
                }
            }

            newColorGradients.Add(0.0f, spectrum.GetColorAt(1 - 1 / cutOffPoint));
        }

        _brush = GradientBrush.GetDrawingBrush();
        ((PathGradientBrush)_brush).TranslateTransform(
            _dimension.X,
            _dimension.Y
        );
        ((PathGradientBrush)_brush).ScaleTransform(_dimension.Width, _dimension.Height);
    }

    public override void Draw(IAuroraBitmap g)
    {
        if (_invalidated)
        {
            VirtUpdate();
            _invalidated = false;
        }

        if(_brush is PathGradientBrush)
        {
            g.Reset();
            g.SetTransform(_transformationMatrix);
            g.FillEllipse(_brush, _dimension);
        }
    }

    public override AnimationFrame BlendWith(AnimationFrame otherAnim, double amount)
    {
        if (otherAnim is not AnimationGradientCircle circleAnim)
        {
            throw new FormatException("Cannot blend with another type");
        }

        amount = GetTransitionValue(amount);

        var newrect = new RectangleF(CalculateNewValue(_dimension.X, circleAnim._dimension.X, amount),
            CalculateNewValue(_dimension.Y, circleAnim._dimension.Y, amount),
            CalculateNewValue(_dimension.Width, circleAnim._dimension.Width, amount),
            CalculateNewValue(_dimension.Height, circleAnim._dimension.Height, amount)
        );

        var newwidth = CalculateNewValue(_width, circleAnim._width, amount);
        var newAngle = CalculateNewValue(_angle, circleAnim._angle, amount);

        return new AnimationGradientCircle(newrect, GradientBrush.BlendEffectBrush(circleAnim.GradientBrush, amount), newwidth).SetAngle(newAngle);
    }

    public override AnimationFrame GetCopy()
    {
        var newrect = new RectangleF(_dimension.X,
            _dimension.Y,
            _dimension.Width,
            _dimension.Height
        );

        return new AnimationGradientCircle(newrect, new EffectBrush(GradientBrush), _width, _duration).SetAngle(_angle).SetTransitionType(_transitionType);
    }

    public override bool Equals(object obj)
    {
        if (ReferenceEquals(null, obj)) return false;
        if (ReferenceEquals(this, obj)) return true;
        if (obj.GetType() != GetType()) return false;
        return Equals((AnimationGradientCircle)obj);
    }

    public bool Equals(AnimationGradientCircle p)
    {
        return _color.Equals(p._color) &&
               _dimension.Equals(p._dimension) &&
               _width.Equals(p._width) &&
               _duration.Equals(p._duration) &&
               _angle.Equals(p._angle) &&
               GradientBrush.Equals(p.GradientBrush);
    }

    public override int GetHashCode()
    {
        unchecked
        {
            var hash = 17;
            hash = hash * 23 + _color.GetHashCode();
            hash = hash * 23 + _dimension.GetHashCode();
            hash = hash * 23 + _width.GetHashCode();
            hash = hash * 23 + _duration.GetHashCode();
            hash = hash * 23 + _angle.GetHashCode();
            hash = hash * 23 + GradientBrush.GetHashCode();
            return hash;
        }
    }

    public override string ToString()
    {
        return $"AnimationGradientCircle [ Color: {_color.ToString()} Dimensions: {_dimension.ToString()} Width: {_width} Duration: {_duration} Angle: {_angle} ]";
    }
}