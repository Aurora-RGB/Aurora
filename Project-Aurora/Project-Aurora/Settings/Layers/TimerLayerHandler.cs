using System;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Timers;
using System.Windows.Controls;
using AuroraRgb.EffectsEngine;
using AuroraRgb.Modules;
using AuroraRgb.Modules.Inputs;
using AuroraRgb.Profiles;
using AuroraRgb.Settings.Layers.Controls;
using AuroraRgb.Settings.Overrides;
using AuroraRgb.Utils;
using Newtonsoft.Json;

namespace AuroraRgb.Settings.Layers;

public sealed class TimerLayerHandlerProperties : LayerHandlerProperties2Color<TimerLayerHandlerProperties> {

    public TimerLayerHandlerProperties()
    { }
    public TimerLayerHandlerProperties(bool assignDefault) : base(assignDefault) { }

    private Keybind[]? _triggerKeys;
    [JsonProperty("_TriggerKeys")]
    public Keybind[] TriggerKeys
    {
        get => Logic._triggerKeys ?? _triggerKeys ?? [];
        set => _triggerKeys = value;
    }

    private int? _duration;
    [LogicOverridable("Duration")]
    [JsonProperty("_Duration")]
    public int Duration
    {
        get => Logic._duration ?? _duration ?? 0;
        set => _duration = value;
    }

    private TimerLayerAnimationType? _animationType;
    [JsonProperty("_AnimationType")]
    [LogicOverridable("Animation Type")]
    public TimerLayerAnimationType AnimationType
    {
        get => Logic._animationType ?? _animationType ?? TimerLayerAnimationType.OnOff;
        set => _animationType = value;
    }

    private TimerLayerRepeatPressAction? _repeatAction;
    [JsonProperty("_RepeatAction")]
    [LogicOverridable("Repeat Action")]
    public TimerLayerRepeatPressAction RepeatAction
    {
        get => Logic._repeatAction ?? _repeatAction ?? TimerLayerRepeatPressAction.Reset;
        set => _repeatAction = value;
    }

    public override void Default() {
        base.Default();
        _triggerKeys = [];
        _duration = 5000;
        _animationType = TimerLayerAnimationType.OnOff;
        _repeatAction = TimerLayerRepeatPressAction.Reset;
    }
}

public class TimerLayerHandler : LayerHandler<TimerLayerHandlerProperties> {
    private readonly CustomTimer _timer;
    private bool _isActive;
    private bool _invalidated;

    public TimerLayerHandler(): base("Timer Layer")
    {
        _timer = new CustomTimer();
        _timer.Trigger += Timer_Elapsed;
    }

    protected override async Task Initialize()
    {
        await base.Initialize();

        (await InputsModule.InputEvents).KeyDown += InputEvents_KeyDown;
    }

    public override void Dispose() {
        InputsModule.InputEvents.Result.KeyDown -= InputEvents_KeyDown;
        _timer.Dispose();
        base.Dispose();
    }

    protected override UserControl CreateControl() {
        return new Control_TimerLayer(this);
    }

    public override EffectLayer Render(IGameState gameState) {
        if (_invalidated)
        {
            EffectLayer.Clear();
        }

        if (!_isActive)
        {
            EffectLayer.Set(Properties.Sequence, Properties.PrimaryColor);
            return EffectLayer;
        }

        switch (Properties.AnimationType)
        {
            case TimerLayerAnimationType.OnOff:
                EffectLayer.Set(Properties.Sequence, Properties.SecondaryColor);
                break;

            case TimerLayerAnimationType.Fade:
                EffectLayer.Set(Properties.Sequence,
                    ColorUtils.BlendColors(Properties.SecondaryColor, Properties.PrimaryColor, _timer.InterpolationValue));
                break;

            case TimerLayerAnimationType.Progress:
            case TimerLayerAnimationType.ProgressGradual:
                EffectLayer.PercentEffect(Properties.SecondaryColor, Properties.PrimaryColor, Properties.Sequence,
                    _timer.InterpolationValue, 1,
                    Properties.AnimationType == TimerLayerAnimationType.Progress
                        ? PercentEffectType.Progressive
                        : PercentEffectType.Progressive_Gradual);
                break;
        }

        return EffectLayer;
    }

    protected override void PropertiesChanged(object? sender, PropertyChangedEventArgs args)
    {
        base.PropertiesChanged(sender, args);

        _invalidated = true;
    }

    private void Timer_Elapsed(object? sender) {
        _isActive = false;
    }

    private void InputEvents_KeyDown(object? sender, KeyboardKeyEventArgs e)
    {
        if (!Array.Exists(Properties.TriggerKeys, keybind => keybind.IsPressed()))
        {
            return;
        }

        switch (Properties.RepeatAction) {
            // Restart the timer from scratch
            case TimerLayerRepeatPressAction.Reset:
                _timer.Reset(Properties.Duration);
                _isActive = true;
                break;

            case TimerLayerRepeatPressAction.Extend:
                _timer.Extend(Properties.Duration);
                _isActive = true;
                break;

            case TimerLayerRepeatPressAction.Cancel:
                if (_isActive)
                    _timer.Stop();
                else
                    _timer.Reset(Properties.Duration);
                _isActive = !_isActive;
                break;
                        
            case TimerLayerRepeatPressAction.Ignore:
                if (!_isActive) {
                    _timer.Reset(Properties.Duration);
                    _isActive = true;
                }
                break;
        }
    }
}

internal sealed class CustomTimer : IDisposable {
    public delegate void TriggerHandler(object? sender);
    public event TriggerHandler? Trigger;

    private readonly Timer _timer;
    private double _max;
    private DateTime _startAt;

    public CustomTimer() {
        _timer = new Timer();
        _timer.AutoReset = false;
        _timer.Elapsed += Timer_Elapsed;
    }

    private void Timer_Elapsed(object? sender, ElapsedEventArgs e) {
        Trigger?.Invoke(this);
        _timer.Enabled = false;
    }

    /// <summary>Stops the timer and restarts it with the given time.</summary>
    private void SetTimer(double t) {
        _timer.Stop();
        _timer.Interval = t;
        _timer.Start();
    }

    public void Reset(double t) {
        SetTimer(t);
        _startAt = DateTime.UtcNow;
        _max = t;
    }

    public void Stop() {
        _timer.Stop();
    }

    public void Extend(double t) {
        // If the timer's not running, behave like Reset
        if (!_timer.Enabled)
            Reset(t);

        // If timer is running
        else {
            _max += t; // extend max
            SetTimer(_max - Current);
        }
    }

    /// <summary>Gets how many milliseconds has elapsed since starting timer.</summary>
    public int Current => (int)(DateTime.UtcNow - _startAt).TotalMilliseconds;

    /// <summary>Gets how far through the timer is as a value between 0 and 1 (for use with the fade animation mode).</summary>
    public double InterpolationValue => Current / _max;

    public void Dispose() {
        _timer.Dispose();
    }
}
    
public enum TimerLayerAnimationType {
    [Description("On/Off")] OnOff,
    Fade,
    Progress,
    [Description("Progress (Gradual)")] ProgressGradual
}

public enum TimerLayerRepeatPressAction {
    [Description("Restart")] Reset,
    [Description("Extend")] Extend,
    [Description("Cancel")] Cancel,
    [Description("Ignore")] Ignore
}