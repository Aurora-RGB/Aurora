﻿using System;
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
    private readonly EffectLayer _layer = new("IDLE", true);

    private long _previousTime = Time.GetMillisecondsSinceEpoch();
    internal long CurrentTime = Time.GetMillisecondsSinceEpoch();

    internal readonly Random Randomizer = new();

    internal readonly LayerEffectConfig EffectCfg = new();

    internal readonly DeviceKeys[] AllKeys = Enum.GetValues(typeof(DeviceKeys)).Cast<DeviceKeys>().ToArray();

    private AwayEffect _awayEffect = new DimEffect();

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
            IdleEffects.Dim => new DimEffect(),
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

    internal float GetDeltaTime()
    {
        return (CurrentTime - _previousTime) / 1000.0f;
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
        CurrentTime = Time.GetMillisecondsSinceEpoch();
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
    protected readonly float IdleFrequency = Global.Configuration.IdleFrequency;
    protected readonly int IdleAmount = Global.Configuration.IdleAmount;
    
    protected readonly Color IdleEffectPrimaryColor = Global.Configuration.IdleEffectPrimaryColor;
    protected readonly SolidBrush IdEffectSecondaryColorBrush = new(Global.Configuration.IdleEffectSecondaryColor);

    public abstract void Update(EffectLayer layer);
}

internal class NoneEffect : AwayEffect
{
    public override void Update(EffectLayer layer)
    {
        //noop
    }
}

internal class DimEffect : AwayEffect
{
    private readonly Brush _dimBrush = new SolidBrush(Color.FromArgb(125, 0, 0, 0));

    public override void Update(EffectLayer layer)
    {
        layer.Fill(_dimBrush);
    }
}

internal class ColorBreathingEffect : AwayEffect
{
    private readonly EventIdle _eventIdle;

    public ColorBreathingEffect(EventIdle eventIdle)
    {
        _eventIdle = eventIdle;
    }

    public override void Update(EffectLayer layer)
    {
        layer.Fill(IdEffectSecondaryColorBrush);
        var sine = (float) Math.Pow(
            Math.Sin((double) (_eventIdle.CurrentTime % 10000L / 10000.0f) * 2 * Math.PI *
                     IdleSpeed), 2);
        layer.FillOver(Color.FromArgb((byte) (sine * 255), IdleEffectPrimaryColor));
    }
}

internal class RainbowShiftHorizontal : AwayEffect
{
    private readonly EventIdle _eventIdle;

    public RainbowShiftHorizontal(EventIdle eventIdle)
    {
        _eventIdle = eventIdle;
    }

    public override void Update(EffectLayer layer)
    {
        layer.DrawGradient(LayerEffects.RainbowShift_Horizontal, _eventIdle.EffectCfg);
    }
}

internal class RainbowShiftVertical : AwayEffect
{
    private readonly EventIdle _eventIdle;

    public RainbowShiftVertical(EventIdle eventIdle)
    {
        _eventIdle = eventIdle;
    }

    public override void Update(EffectLayer layer)
    {
        layer.DrawGradient(LayerEffects.RainbowShift_Vertical, _eventIdle.EffectCfg);
    }
}

internal class StarFall : AwayEffect
{
    private readonly EventIdle _eventIdle;
    private long _nextStarSet;

    private Dictionary<DeviceKeys, float> _stars = new();

    public StarFall(EventIdle eventIdle)
    {
        _eventIdle = eventIdle;
    }

    public override void Update(EffectLayer layer)
    {
        if (_nextStarSet < _eventIdle.CurrentTime)
        {
            for (var x = 0; x < IdleAmount; x++)
            {
                var star = _eventIdle.AllKeys[_eventIdle.Randomizer.Next(_eventIdle.AllKeys.Length)];
                _stars[star] = 1.0f;
            }

            _nextStarSet = _eventIdle.CurrentTime + (long) (1000L * IdleFrequency);
        }

        layer.Fill(IdEffectSecondaryColorBrush);

        var starsKeys = _stars.Keys.ToArray();
        foreach (var star in starsKeys)
        {
            layer.Set(star,
                ColorUtils.BlendColors(Color.Black, IdleEffectPrimaryColor, _stars[star]));
            _stars[star] -= _eventIdle.GetDeltaTime() * 0.05f * IdleSpeed;
        }
    }
}

internal class RainFall : AwayEffect
{
    private readonly EventIdle _eventIdle;

    private readonly Dictionary<DeviceKeys, float> _raindrops = new();
    private long _nextStarSet;

    private readonly ColorSpectrum _dropSpec = new(Global.Configuration.IdleEffectPrimaryColor,
        Color.FromArgb(0, Global.Configuration.IdleEffectPrimaryColor));

    public RainFall(EventIdle eventIdle)
    {
        _eventIdle = eventIdle;
    }

    public override void Update(EffectLayer layer)
    {
        if (_nextStarSet < _eventIdle.CurrentTime)
        {
            for (var x = 0; x < IdleAmount; x++)
            {
                var star = _eventIdle.AllKeys[_eventIdle.Randomizer.Next(_eventIdle.AllKeys.Length)];
                _raindrops[star] = 1.0f;
            }

            _nextStarSet = _eventIdle.CurrentTime + (long) (1000L * IdleFrequency);
        }

        layer.Fill(IdEffectSecondaryColorBrush);

        var raindropsKeys = _raindrops.Keys.ToArray();

        using var g = layer.GetGraphics();
        foreach (var raindrop in raindropsKeys)
        {
            var pt = Effects.Canvas.GetRectangle(raindrop).Center;

            var transitionValue = 1.0f - _raindrops[raindrop];
            var radius = transitionValue * Effects.Canvas.BiggestSize;

            g.DrawEllipse(new Pen(_dropSpec.GetColorAt(transitionValue), 2),
                pt.X - radius,
                pt.Y - radius,
                2 * radius,
                2 * radius);

            _raindrops[raindrop] -= _eventIdle.GetDeltaTime() * 0.05f * IdleSpeed;
        }
    }
}

internal class Blackout : AwayEffect
{
    public override void Update(EffectLayer layer)
    {
        layer.Fill(Brushes.Black);
    }
}

public class Matrix : AwayEffect
{
    private readonly EventIdle _eventIdle;

    private readonly AnimationMix _matrixLines = new AnimationMix().SetAutoRemove(true); //This will be an infinite Mix
    private long _nextStarSet;

    public Matrix(EventIdle eventIdle)
    {
        _eventIdle = eventIdle;
    }

    public override void Update(EffectLayer layer)
    {
        if (_nextStarSet < _eventIdle.CurrentTime)
        {
            var darkerPrimary = ColorUtils.MultiplyColorByScalar(IdleEffectPrimaryColor, 0.50);

            for (var x = 0; x < IdleAmount; x++)
            {
                var widthStart = _eventIdle.Randomizer.Next(Effects.Canvas.Width);
                var delay = _eventIdle.Randomizer.Next(550) / 100.0f;
                var randomId = _eventIdle.Randomizer.Next(125536789);

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
                            _eventIdle.CurrentTime % 1000000L / 1000.0f + delay
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
                        _eventIdle.CurrentTime % 1000000L / 1000.0f + delay
                    );

                _matrixLines.AddTrack(matrixLine);
                _matrixLines.AddTrack(matrixLineTrail);
            }

            _nextStarSet = _eventIdle.CurrentTime + (long) (1000L * IdleFrequency);
        }

        layer.Fill(IdEffectSecondaryColorBrush);

        using var g = layer.GetGraphics();
        _matrixLines.Draw(g, _eventIdle.CurrentTime % 1000000L / 1000.0f);
    }
}

internal class RainFallSmooth : AwayEffect
{
    private readonly EventIdle _eventIdle;

    private readonly ColorSpectrum _dropSpec = new(Global.Configuration.IdleEffectPrimaryColor,
        Color.FromArgb(0, Global.Configuration.IdleEffectPrimaryColor));

    private long _nextStarSet;

    public RainFallSmooth(EventIdle eventIdle)
    {
        _eventIdle = eventIdle;
    }

    private Dictionary<DeviceKeys, float> _raindrops = new();

    public override void Update(EffectLayer layer)
    {
        if (_nextStarSet < _eventIdle.CurrentTime)
        {
            for (var x = 0; x < IdleAmount; x++)
            {
                var star = _eventIdle.AllKeys[_eventIdle.Randomizer.Next(_eventIdle.AllKeys.Length)];
                _raindrops[star] = 1.0f;
            }

            _nextStarSet = _eventIdle.CurrentTime + (long) (1000L * IdleFrequency);
        }

        layer.FillOver(IdEffectSecondaryColorBrush);

        var drops = _raindrops.Keys.Select(d =>
        {
            var pt = Effects.Canvas.GetRectangle(d).Center;
            var transitionValue = 1.0f - _raindrops[d];
            var radius = transitionValue * Effects.Canvas.BiggestSize;
            _raindrops[d] -= _eventIdle.GetDeltaTime() * 0.05f * IdleSpeed;
            return new Tuple<DeviceKeys, PointF, float, float>(d, pt, transitionValue, radius);
        }).Where(d => d.Item3 <= 1.5).ToArray();

        const float circleHalfThickness = 1f;

        foreach (var key in _eventIdle.AllKeys)
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
                var bg = layer.Get(key);
                var fg = _dropSpec.GetColorAt(raindrop.Item3);
                layer.Set(key, (Color)ColorUtils.BlendColors(
                    ColorUtils.DrawingToSimpleColor(bg),
                    ColorUtils.DrawingToSimpleColor(fg),
                    percent));
            }
        }
    }
}