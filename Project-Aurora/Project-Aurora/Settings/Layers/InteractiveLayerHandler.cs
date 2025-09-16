﻿using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using AuroraRgb.EffectsEngine;
using AuroraRgb.EffectsEngine.Animations;
using AuroraRgb.Modules;
using AuroraRgb.Modules.Inputs;
using AuroraRgb.Profiles;
using AuroraRgb.Profiles.Desktop;
using AuroraRgb.Settings.Layers.Controls;
using AuroraRgb.Settings.Overrides;
using AuroraRgb.Utils;
using Common.Devices;
using Common.Utils;
using Newtonsoft.Json;
using UserControl = System.Windows.Controls.UserControl;

namespace AuroraRgb.Settings.Layers;

public class InputItem
{
    public enum InputType
    {
        AnimationMix,
        Spectrum
    }

    public readonly DeviceKeys Key;
    public float Progress;
    public bool WaitOnKeyUp;
    public readonly AnimationMix Animation;
    public readonly ColorSpectrum Spectrum;
    public readonly InputType Type;

    public InputItem(DeviceKeys key, float progress, bool waitOnKeyUp, AnimationMix animation)
    {
        Key = key;
        Progress = progress;
        WaitOnKeyUp = waitOnKeyUp;
        Animation = animation;

        Type = InputType.AnimationMix;
    }

    public InputItem(DeviceKeys key, float progress, bool waitOnKeyUp, ColorSpectrum spectrum)
    {
        Key = key;
        Progress = progress;
        WaitOnKeyUp = waitOnKeyUp;
        Spectrum = spectrum;

        Type = InputType.Spectrum;
    }
}

public sealed partial class InteractiveLayerHandlerProperties : LayerHandlerProperties2Color
{
    private bool? _randomPrimaryColor;
    [JsonProperty("_RandomPrimaryColor")]
    public bool RandomPrimaryColor
    {
        get => Logic?._randomPrimaryColor ?? _randomPrimaryColor ?? false;
        set => _randomPrimaryColor = value;
    }

    private bool? _randomSecondaryColor;
    [JsonProperty("_RandomSecondaryColor")]
    public bool RandomSecondaryColor
    {
        get => Logic?._randomSecondaryColor ?? _randomSecondaryColor ?? false;
        set => _randomSecondaryColor = value;
    }

    private float? _effectSpeed;
    [LogicOverridable("Effect Speed")]
    [JsonProperty("_EffectSpeed")]
    public float EffectSpeed
    {
        get => Logic?._effectSpeed ?? _effectSpeed ?? 0.0f;
        set => _effectSpeed = value;
    }

    private bool? _waitOnKeyUp;
    [JsonProperty("_WaitOnKeyUp")]
    public bool WaitOnKeyUp
    {
        get => Logic?._waitOnKeyUp ?? _waitOnKeyUp ?? false;
        set => _waitOnKeyUp = value;
    }

    private InteractiveEffects? _interactiveEffect;
    [LogicOverridable("Interactive Effect")]
    [JsonProperty("_InteractiveEffect")]
    public InteractiveEffects InteractiveEffect
    {
        get => Logic?._interactiveEffect ?? _interactiveEffect ?? InteractiveEffects.None;
        set => _interactiveEffect = value;
    }

    private int? _effectWidth;
    [LogicOverridable("Effect Width")]
    [JsonProperty("_EffectWidth")]
    public int EffectWidth
    {
        get => Logic?._effectWidth ?? _effectWidth ?? 0;
        set => _effectWidth = value;
    }

    private bool? _usePressBuffer;
    [JsonProperty("_UsePressBuffer")]
    public bool? UsePressBuffer
    {
        get => Logic?._usePressBuffer ?? _usePressBuffer ?? true;
        set => _usePressBuffer = value ?? false;
    }

    private DeviceKeys? _mouseEffectKey;
    public DeviceKeys MouseEffectKey
    {
        get => (Logic?._mouseEffectKey ?? _mouseEffectKey) ?? DeviceKeys.NONE;
        set => SetFieldAndRaisePropertyChanged(out _mouseEffectKey, value);
    }

    public override void Default()
    {
        base.Default();
        _randomPrimaryColor = false;
        _randomSecondaryColor = false;
        _waitOnKeyUp = false;
        _effectSpeed = 1.0f;
        _interactiveEffect = InteractiveEffects.None;
        _effectWidth = 2;
        _usePressBuffer = true;
    }
}

public sealed class InteractiveLayerHandler : LayerHandler<InteractiveLayerHandlerProperties, BitmapEffectLayer>
{
    private readonly Func<KeyValuePair<DeviceKeys, long>, bool> _keysToRemove;
    private readonly ConcurrentDictionary<DeviceKeys, InputItem> _inputDictionary = new(new Dictionary<DeviceKeys, InputItem>(Effects.MaxDeviceId));
        
    private long _previousTime;
    private long _currentTime;

    public InteractiveLayerHandler(): base("Interactive Effects")
    {
        _keysToRemove = lengthPresses => !Properties.UsePressBuffer.GetValueOrDefault() || _currentTime - lengthPresses.Value > PressBuffer;
    }

    protected override async Task Initialize()
    {
        await base.Initialize();
            
        (await InputsModule.InputEvents).KeyDown += InputEventsKeyDown;
        (await InputsModule.InputEvents).KeyUp += InputEventsKeyUp;
        (await InputsModule.InputEvents).MouseButtonDown += MouseKeyDown;
        (await InputsModule.InputEvents).MouseButtonUp += MouseKeyUp;
    }

    private float GetDeltaTime()
    {
        return (_currentTime - _previousTime) / 1000.0f;
    }

    protected override UserControl CreateControl()
    {
        return new Control_InteractiveLayer(this);
    }

    private void MouseKeyUp(object? sender, EventArgs mouseInputEventArgs)
    {
        var mouseKey = Properties.MouseEffectKey;
        if (mouseKey == DeviceKeys.NONE)
            return;

        DeviceKeyUp(mouseKey);
    }

    private void InputEventsKeyUp(object? sender, KeyboardKeyEventArgs e)
    {
        var deviceKey = e.GetDeviceKey();
        DeviceKeyUp(deviceKey);
    }

    private void DeviceKeyUp(DeviceKeys deviceKey)
    {
        if (deviceKey == DeviceKeys.NONE) return;

        if (_inputDictionary.TryGetValue(deviceKey, out var value))
        {
            value.WaitOnKeyUp = false;
        }
    }

    private readonly ConcurrentDictionary<DeviceKeys, long> _timeOfLastPress = new();
    private const long PressBuffer = 300L;

    private void MouseKeyDown(object? sender, EventArgs mouseInputEventArgs)
    {
        var mouseKey = Properties.MouseEffectKey;
        if (mouseKey == DeviceKeys.NONE)
            return;

        DeviceKeyDown(mouseKey);
    }

    private void InputEventsKeyDown(object? sender, KeyboardKeyEventArgs e)
    {
        var deviceKey = e.GetDeviceKey();

        DeviceKeyDown(deviceKey);
    }

    private void DeviceKeyDown(DeviceKeys deviceKey)
    {
        if (Time.GetMillisecondsSinceEpoch() - _previousTime > 1000L)
            return; //This event wasn't used for at least 1 second

        var currentTime = Time.GetMillisecondsSinceEpoch();

        if (_timeOfLastPress.TryGetValue(deviceKey, out var pressTime))
        {
            if (Properties.UsePressBuffer.GetValueOrDefault() &&
                (currentTime = Time.GetMillisecondsSinceEpoch()) - pressTime < PressBuffer)
                return;
            _timeOfLastPress.TryRemove(deviceKey, out _);
        }

        if (deviceKey == DeviceKeys.NONE || Properties.Sequence.Keys.Contains(deviceKey)) return;
        var pt = Effects.Canvas.GetRectangle(deviceKey).Center;
        if (pt.IsEmpty) return;

        _timeOfLastPress.TryAdd(deviceKey, currentTime);

        _inputDictionary[deviceKey] = CreateInputItem(deviceKey, pt);
    }

    private InputItem CreateInputItem(DeviceKeys key, PointF origin)
    {
        var primaryC = Properties.RandomPrimaryColor ? CommonColorUtils.GenerateRandomColor() : Properties.PrimaryColor;
        var secondaryC = Properties.RandomSecondaryColor ? CommonColorUtils.GenerateRandomColor() : Properties.SecondaryColor;

        var animMix = new AnimationMix();

        switch (Properties.InteractiveEffect)
        {
            case InteractiveEffects.Wave:
            {
                AnimationTrack wave = new AnimationTrack("Wave effect", 1.0f);
                wave.SetFrame(0.0f,
                    new AnimationCircle(origin, 0, primaryC, Properties.EffectWidth)
                );
                wave.SetFrame(0.80f,
                    new AnimationCircle(origin, Effects.Canvas.Width * 0.80f, secondaryC, Properties.EffectWidth)
                );
                wave.SetFrame(1.00f,
                    new AnimationCircle(origin, Effects.Canvas.Width + (float)Properties.EffectWidth / 2, Color.FromArgb(0, secondaryC), Properties.EffectWidth)
                );
                animMix.AddTrack(wave);
                break;
            }
            case InteractiveEffects.WaveRainbow:
            {
                var rainbowWave = new AnimationTrack("Rainbow Wave", 1.0f);

                rainbowWave.SetFrame(0.0f, new AnimationGradientCircle(origin, 0,
                    new EffectBrush(new ColorSpectrum(ColorSpectrum.Rainbow).Flip(), EffectBrush.BrushType.Radial), Properties.EffectWidth));
                rainbowWave.SetFrame(1.0f, new AnimationGradientCircle(origin, Effects.Canvas.Width + (float)Properties.EffectWidth / 2,
                    new EffectBrush(new ColorSpectrum(ColorSpectrum.Rainbow).Flip(), EffectBrush.BrushType.Radial), Properties.EffectWidth));

                animMix.AddTrack(rainbowWave);
                break;
            }
            case InteractiveEffects.WaveFilled:
            {
                var wave = new AnimationTrack("Filled Wave effect", 1.0f);
                wave.SetFrame(0.0f,
                    new AnimationFilledCircle(origin, 0, primaryC, Properties.EffectWidth)
                );
                wave.SetFrame(0.80f,
                    new AnimationFilledCircle(origin, Effects.Canvas.Width * 0.80f, secondaryC, Properties.EffectWidth)
                );
                wave.SetFrame(1.00f,
                    new AnimationFilledCircle(origin, Effects.Canvas.Width + (float)Properties.EffectWidth / 2, Color.FromArgb(0, secondaryC), Properties.EffectWidth)
                );
                animMix.AddTrack(wave);
                break;
            }
            case InteractiveEffects.KeyPress:
            {
                ColorSpectrum spec;
                spec = new ColorSpectrum(primaryC, Color.FromArgb(0, secondaryC));
                spec.SetColorAt(0.80f, secondaryC);

                return new InputItem(key, 0.0f, Properties.WaitOnKeyUp, spec);
            }
            case InteractiveEffects.ArrowFlow:
            {
                var arrow = new AnimationTrack("Arrow Flow effect", 1.0f);
                arrow.SetFrame(0.0f,
                    new AnimationLines(
                        new[] {
                            new AnimationLine(origin, origin, primaryC, Properties.EffectWidth),
                            new AnimationLine(origin, origin, primaryC, Properties.EffectWidth)
                        }
                    )
                );
                arrow.SetFrame(0.33f,
                    new AnimationLines(
                        new[] {
                            new AnimationLine(origin, origin with { X = origin.X + Effects.Canvas.Width * 0.33f }, ColorUtils.BlendColors(primaryC, secondaryC, 0.33D), Properties.EffectWidth),
                            new AnimationLine(origin, origin with { X = origin.X - Effects.Canvas.Width * 0.33f }, ColorUtils.BlendColors(primaryC, secondaryC, 0.33D), Properties.EffectWidth)
                        }
                    )
                );
                arrow.SetFrame(0.66f,
                    new AnimationLines(
                        new[] {
                            new AnimationLine(origin with { X = origin.X + Effects.Canvas.Width * 0.33f }, origin with { X = origin.X + Effects.Canvas.Width * 0.66f }, secondaryC, Properties.EffectWidth),
                            new AnimationLine(origin with { X = origin.X - Effects.Canvas.Width * 0.33f }, origin with { X = origin.X - Effects.Canvas.Width * 0.66f }, secondaryC, Properties.EffectWidth)
                        }
                    )
                );
                arrow.SetFrame(1.0f,
                    new AnimationLines(
                        new[] {
                            new AnimationLine(origin with { X = origin.X + Effects.Canvas.Width * 0.66f }, origin with { X = origin.X + Effects.Canvas.Width }, Color.FromArgb(0, secondaryC), Properties.EffectWidth),
                            new AnimationLine(origin with { X = origin.X - Effects.Canvas.Width * 0.66f }, origin with { X = origin.X - Effects.Canvas.Width }, Color.FromArgb(0, secondaryC), Properties.EffectWidth)
                        }
                    )
                );
                animMix.AddTrack(arrow);
                break;
            }
        }

        return new InputItem(key, 0.0f, Properties.WaitOnKeyUp, animMix);
    }

    public override EffectLayer Render(IGameState gamestate)
    {
        _previousTime = _currentTime;
        _currentTime = Time.GetMillisecondsSinceEpoch();
        foreach (var lengthPresses in _timeOfLastPress.Where(_keysToRemove))
        {
            _timeOfLastPress.TryRemove(lengthPresses.Key, out _);
        }

        if (_inputDictionary.Values.Count == 0)
        {
            return EmptyLayer.Instance;
        }

        EffectLayer.Clear();
        foreach (var input in _inputDictionary.Values)
        {
            try
            {
                switch (input.Type)
                {
                    case InputItem.InputType.Spectrum:
                    default:
                    {
                        var transitionValue = input.Progress / Effects.Canvas.Width;

                        if (transitionValue > 1.0f)
                            continue;

                        var color = input.Spectrum.GetColorAt(transitionValue);

                        EffectLayer.Set(input.Key, color);
                        break;
                    }
                    case InputItem.InputType.AnimationMix:
                    {
                        var timeValue = input.Progress / Effects.Canvas.Width;

                        if (timeValue > 1.0f)
                            continue;

                        var graphics = EffectLayer.GetGraphics();
                        input.Animation.Draw(graphics, timeValue);
                        break;
                    }
                }
            }
            catch (Exception exc)
            {
                Global.logger.Error(exc, "Interactive layer exception");
            }
        }

        foreach (var kv in _inputDictionary)
        {
            if (kv.Value.Progress > Effects.Canvas.Width)
                _inputDictionary.Remove(kv.Key, out _);
            else
            {
                if (kv.Value.WaitOnKeyUp) continue;
                var transAdded = Properties.EffectSpeed * (GetDeltaTime() * 5.0f);
                kv.Value.Progress += transAdded;
            }
        }

        return EffectLayer;
    }

    public override void Dispose()
    {
        var inputEvents = InputsModule.InputEvents.Result;
        inputEvents.KeyDown -= InputEventsKeyDown;
        inputEvents.KeyUp -= InputEventsKeyUp;
        inputEvents.MouseButtonDown -= MouseKeyDown;
        inputEvents.MouseButtonUp -= MouseKeyUp;

        EffectLayer.Dispose();
        base.Dispose();
    }
}