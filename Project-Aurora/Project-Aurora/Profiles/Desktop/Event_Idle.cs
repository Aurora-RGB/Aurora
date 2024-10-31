using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using AuroraRgb.EffectsEngine;
using AuroraRgb.EffectsEngine.Animations;
using AuroraRgb.Settings;
using AuroraRgb.Utils;
using Common.Devices;

namespace AuroraRgb.Profiles.Desktop;

public sealed class EventIdle : LightEvent
{
    private readonly BitmapEffectLayer _layer = new("IDLE", true);

    private DateTime _previousTime = DateTime.UtcNow;
    internal DateTime CurrentTime = DateTime.UtcNow;

    internal readonly Random Randomizer = new();

    internal readonly LayerEffectConfig EffectCfg = new();

    internal readonly DeviceKeys[] AllKeys = Enum.GetValues(typeof(DeviceKeys)).Cast<DeviceKeys>().ToArray();

    private AwayEffect _awayEffect = DimEffect.Instance;

    public EventIdle()
    {
        Global.Configuration.PropertyChanged += IdleTypeChanged;
        IdleTypeChanged(this, new PropertyChangedEventArgs(nameof(Global.Configuration.IdleType)));
    }

    public override void Dispose()
    {
        Global.Configuration.PropertyChanged -= IdleTypeChanged;
    }

    private bool _invalidated;

    private void IdleTypeChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName != nameof(Global.Configuration.IdleType))
        {
            return;
        }
        EffectCfg.Speed = Global.Configuration.IdleSpeed;
        _invalidated = true;

        _awayEffect = Global.Configuration.IdleType switch
        {
            IdleEffects.None => new NoneEffect(),
            IdleEffects.Dim => DimEffect.Instance,
            IdleEffects.ColorBreathing => new ColorBreathingEffect(this),
            IdleEffects.RainbowShift_Horizontal => new RainbowShiftHorizontal(this),
            IdleEffects.RainbowShift_Vertical => new RainbowShiftVertical(this),
            IdleEffects.StarFall => new StarFall(this),
            IdleEffects.RainFall => new RainFall(this),
            IdleEffects.Blackout => new Blackout(),
            IdleEffects.Matrix => new Matrix(this),
            IdleEffects.RainFallSmooth => new RainFallSmooth(this),
            _ => _awayEffect
        };
    }

    internal TimeSpan GetDeltaTime()
    {
        return CurrentTime - _previousTime;
    }

    public override void UpdateLights(EffectFrame frame)
    {
        if (Global.Configuration.IdleType == IdleEffects.None)
        {
            return;
        }
        if (_invalidated)
        {
            _layer.Fill(Brushes.Transparent);
            _invalidated = false;
        }

        _previousTime = CurrentTime;
        CurrentTime = DateTime.UtcNow;
        _awayEffect.Update(_layer);

        frame.AddOverlayLayer(_layer);
    }

    public override void SetGameState(IGameState newGameState)
    {
        //This event does not take a game state
    }
}

public abstract class AwayEffect
{
    protected readonly float IdleSpeed = Global.Configuration.IdleSpeed;
    protected readonly TimeSpan IdleFrequency = TimeSpan.FromSeconds(Global.Configuration.IdleFrequency);
    protected readonly int IdleAmount = Global.Configuration.IdleAmount;
    
    protected readonly Color IdleEffectPrimaryColor = Global.Configuration.IdleEffectPrimaryColor;
    protected readonly SolidBrush IdEffectSecondaryColorBrush = new(Global.Configuration.IdleEffectSecondaryColor);

    public abstract void Update(BitmapEffectLayer layer);
}

internal class NoneEffect : AwayEffect
{
    public override void Update(BitmapEffectLayer layer)
    {
        //noop
    }
}

internal class DimEffect : AwayEffect
{
    public static DimEffect Instance { get; } = new();
    private readonly Brush _dimBrush = new SolidBrush(Color.FromArgb(125, 0, 0, 0));

    private DimEffect()
    {
    }

    public override void Update(BitmapEffectLayer layer)
    {
        layer.Fill(_dimBrush);
    }
}

internal class ColorBreathingEffect(EventIdle eventIdle) : AwayEffect
{
    public override void Update(BitmapEffectLayer layer)
    {
        layer.Fill(IdEffectSecondaryColorBrush);
        var sine = (float) Math.Pow(
            Math.Sin((double) (eventIdle.CurrentTime.Millisecond % 10000L / 10000.0f) * 2 * Math.PI *
                     IdleSpeed), 2);
        layer.FillOver(Color.FromArgb((byte) (sine * 255), IdleEffectPrimaryColor));
    }
}

internal class RainbowShiftHorizontal(EventIdle eventIdle) : AwayEffect
{
    public override void Update(BitmapEffectLayer layer)
    {
        layer.DrawGradient(LayerEffects.RainbowShift_Horizontal, eventIdle.EffectCfg);
    }
}

internal class RainbowShiftVertical(EventIdle eventIdle) : AwayEffect
{
    public override void Update(BitmapEffectLayer layer)
    {
        layer.DrawGradient(LayerEffects.RainbowShift_Vertical, eventIdle.EffectCfg);
    }
}

internal class StarFall(EventIdle eventIdle) : AwayEffect
{
    private DateTime _nextStarSet;

    private readonly Dictionary<DeviceKeys, double> _stars = new();

    public override void Update(BitmapEffectLayer layer)
    {
        if (_nextStarSet < eventIdle.CurrentTime)
        {
            for (var x = 0; x < IdleAmount; x++)
            {
                var star = eventIdle.AllKeys[eventIdle.Randomizer.Next(eventIdle.AllKeys.Length)];
                _stars[star] = 1.0;
            }

            _nextStarSet = eventIdle.CurrentTime + IdleFrequency;
        }

        layer.Fill(IdEffectSecondaryColorBrush);

        var starsKeys = _stars.Keys.ToArray();
        foreach (var star in starsKeys)
        {
            layer.Set(star,
                ColorUtils.BlendColors(Color.Black, IdleEffectPrimaryColor, _stars[star]));
            _stars[star] -= eventIdle.GetDeltaTime().TotalSeconds * 0.05f * IdleSpeed;
        }
    }
}

internal class RainFall(EventIdle eventIdle) : AwayEffect
{
    private readonly Dictionary<DeviceKeys, double> _raindrops = new();
    private DateTime _nextStarSet;

    private readonly ColorSpectrum _dropSpec = new(Global.Configuration.IdleEffectPrimaryColor,
        Color.FromArgb(0, Global.Configuration.IdleEffectPrimaryColor));

    private readonly Pen _pen = new(Color.Transparent, 2);

    public override void Update(BitmapEffectLayer layer)
    {
        if (_nextStarSet < eventIdle.CurrentTime)
        {
            for (var x = 0; x < IdleAmount; x++)
            {
                var star = eventIdle.AllKeys[eventIdle.Randomizer.Next(eventIdle.AllKeys.Length)];
                _raindrops[star] = 1.0f;
            }

            _nextStarSet = eventIdle.CurrentTime + IdleFrequency;
        }

        layer.Fill(IdEffectSecondaryColorBrush);

        var raindropsKeys = _raindrops.Keys.ToArray();

        var g = layer.GetGraphics();
        foreach (var raindrop in raindropsKeys)
        {
            var pt = Effects.Canvas.GetRectangle(raindrop).Center;

            var transitionValue = (float)(1.0f - _raindrops[raindrop]);
            var radius = transitionValue * Effects.Canvas.BiggestSize;

            _pen.Color = _dropSpec.GetColorAt(transitionValue);
            g.DrawEllipse(_pen,
                new RectangleF(pt.X - radius, pt.Y - radius, 2 * radius, 2 * radius)
            );

            _raindrops[raindrop] -= eventIdle.GetDeltaTime().TotalSeconds * 0.05f * IdleSpeed;
        }
    }
}

internal class Blackout : AwayEffect
{
    public override void Update(BitmapEffectLayer layer)
    {
        layer.Fill(Brushes.Black);
    }
}

public class Matrix(EventIdle eventIdle) : AwayEffect
{
    private readonly AnimationMix _matrixLines = new AnimationMix().SetAutoRemove(true); //This will be an infinite Mix
    private DateTime _nextStarSet;

    public override void Update(BitmapEffectLayer layer)
    {
        var span = eventIdle.CurrentTime - DateTime.UnixEpoch;
        var ms = (long)span.TotalMilliseconds;

        if (_nextStarSet < eventIdle.CurrentTime)
        {
            var darkerPrimary = ColorUtils.MultiplyColorByScalar(IdleEffectPrimaryColor, 0.50);

            for (var x = 0; x < IdleAmount; x++)
            {
                var widthStart = eventIdle.Randomizer.Next(Effects.Canvas.Width);
                var delay = eventIdle.Randomizer.Next(550) / 100.0f;
                var randomId = eventIdle.Randomizer.Next(125536789);

                //Create animation
                var matrixLine =
                    new AnimationTrack("Matrix Line (Head) " + randomId, 0.0f).SetFrame(
                            0.0f * 1.0f / (0.05f * IdleSpeed),
                            new AnimationLine(widthStart, -3, widthStart, 0,
                                IdleEffectPrimaryColor,
                                3))
                        .SetFrame(
                            0.5f * 1.0f / (0.05f * IdleSpeed),
                            new AnimationLine(widthStart, Effects.Canvas.Height, widthStart, Effects.Canvas.Height + 3,
                                IdleEffectPrimaryColor, 3)).SetShift(
                            ms % 1000000L / 1000.0f + delay
                        );

                var matrixLineTrail =
                    new AnimationTrack("Matrix Line (Trail) " + randomId, 0.0f).SetFrame(
                        0.0f * 1.0f / (0.05f * IdleSpeed),
                        new AnimationLine(widthStart, -12, widthStart, -3, darkerPrimary, 3)).SetFrame(
                        0.5f * 1.0f / (0.05f * IdleSpeed),
                        new AnimationLine(widthStart, Effects.Canvas.Height - 12, widthStart, Effects.Canvas.Height,
                            darkerPrimary, 3)).SetFrame(
                        0.75f * 1.0f / (0.05f * IdleSpeed),
                        new AnimationLine(widthStart, Effects.Canvas.Height, widthStart, Effects.Canvas.Height,
                            darkerPrimary,
                            3)).SetShift(
                        ms % 1000000L / 1000.0f + delay
                    );

                _matrixLines.AddTrack(matrixLine);
                _matrixLines.AddTrack(matrixLineTrail);
            }

            _nextStarSet = eventIdle.CurrentTime + IdleFrequency;
        }

        layer.Fill(IdEffectSecondaryColorBrush);

        var g = layer.GetGraphics();
        _matrixLines.Draw(g, ms % 1000000L / 1000.0f);
    }
}

internal class RainFallSmooth(EventIdle eventIdle) : AwayEffect
{
    private readonly ColorSpectrum _dropSpec = new(Global.Configuration.IdleEffectPrimaryColor,
        Color.FromArgb(0, Global.Configuration.IdleEffectPrimaryColor));

    private readonly Dictionary<DeviceKeys, double> _raindrops = new();

    private DateTime _nextStarSet;

    public override void Update(BitmapEffectLayer layer)
    {
        if (_nextStarSet < eventIdle.CurrentTime)
        {
            for (var x = 0; x < IdleAmount; x++)
            {
                var star = eventIdle.AllKeys[eventIdle.Randomizer.Next(eventIdle.AllKeys.Length)];
                _raindrops[star] = 1.0f;
            }

            _nextStarSet = eventIdle.CurrentTime + IdleFrequency;
        }

        layer.FillOver(IdEffectSecondaryColorBrush);

        var drops = _raindrops.Keys.Select(d =>
        {
            var pt = Effects.Canvas.GetRectangle(d).Center;
            var transitionValue = (float)(1.0f - _raindrops[d]);
            var radius = transitionValue * Effects.Canvas.BiggestSize;
            _raindrops[d] -= eventIdle.GetDeltaTime().TotalMilliseconds * 0.05f * IdleSpeed;
            return new Tuple<DeviceKeys, PointF, float, float>(d, pt, transitionValue, radius);
        }).Where(d => d.Item3 <= 1.5).ToArray();

        const float circleHalfThickness = 1f;

        foreach (var key in eventIdle.AllKeys)
        {
            var keyInfo = Effects.Canvas.GetRectangle(key);

            // For easy calculation every button considered as circle with this radius
            var btnRadius = (keyInfo.Width + keyInfo.Height) / 4f;
            if (btnRadius <= 0) continue;

            foreach (var raindrop in drops)
            {
                var circleInEdge = raindrop.Item4 - circleHalfThickness;
                var circleOutEdge = raindrop.Item4 + circleHalfThickness;
                circleInEdge *= circleInEdge;
                circleOutEdge *= circleOutEdge;

                var xKey = Math.Abs(keyInfo.Center.X - raindrop.Item2.X);
                var yKey = Math.Abs(keyInfo.Center.Y - raindrop.Item2.Y);
                var xKeyInEdge = xKey - btnRadius;
                var xKeyOutEdge = xKey + btnRadius;
                var yKeyInEdge = yKey - btnRadius;
                var yKeyOutEdge = yKey + btnRadius;
                var keyInEdge = xKeyInEdge * xKeyInEdge + yKeyInEdge * yKeyInEdge;
                var keyOutEdge = xKeyOutEdge * xKeyOutEdge + yKeyOutEdge * yKeyOutEdge;

                var btnDiameter = keyOutEdge - keyInEdge;
                var inEdgePercent = (circleOutEdge - keyInEdge) / btnDiameter;
                var outEdgePercent = (keyOutEdge - circleInEdge) / btnDiameter;
                var percent = Math.Min(1, Math.Max(0, inEdgePercent))
                    + Math.Min(1, Math.Max(0, outEdgePercent)) - 1f;

                if (percent <= 0) continue;
                var bg = Color.Transparent;
                var fg = _dropSpec.GetColorAt(raindrop.Item3);
                var blendColor = (Color)ColorUtils.BlendColors(
                    ColorUtils.DrawingToSimpleColor(bg),
                    ColorUtils.DrawingToSimpleColor(fg),
                    percent);
                layer.Set(key, blendColor);
            }
        }
    }
}