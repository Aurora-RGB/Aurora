using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using AuroraRgb.Profiles;
using AuroraRgb.Utils;

namespace AuroraRgb.Settings.Overrides.Logic;

/// <summary>
/// Extends the length of a boolean 'true' state. For example, if the sub-evaluatable evaluated to true for 1 second and this had it's
/// <see cref="ExtensionTime"/> set to 3 seconds, the result of this would be a 3 second long "true" signal. Receiving another true
/// signal while already extending an existing signal will restart the timer.
/// </summary>
[Evaluatable("True Extender", category: EvaluatableCategory.Logic)]
public class BooleanExtender : BoolEvaluatable {

    private readonly Stopwatch _sw = new();
    private bool _lastTimerValue;

    public BooleanExtender() { }
    public BooleanExtender(Evaluatable<bool> evaluatable) { Evaluatable = evaluatable; }
    public BooleanExtender(Evaluatable<bool> evaluatable, double time, TimeUnit timeUnit = TimeUnit.Seconds) : this(evaluatable) { ExtensionTime = time; TimeUnit = timeUnit; }

    public Evaluatable<bool> Evaluatable { get; set; } = new BooleanConstant();
    public double ExtensionTime { get; set; } = 5;
    public TimeUnit TimeUnit { get; set; } = TimeUnit.Seconds;

    public override Visual GetControl() => new StackPanel()
        .WithChild(new StackPanel { Orientation = Orientation.Horizontal }
            .WithChild(new TextBlock { Text = "Extend", Margin = new Thickness(0, 0, 6, 0), VerticalAlignment = VerticalAlignment.Center })
            .WithChild(new Control_EvaluatablePresenter { EvalType = typeof(bool) }
                .WithBinding(Control_EvaluatablePresenter.ExpressionProperty, new Binding("Evaluatable") { Source = this, Mode = BindingMode.TwoWay })))
        .WithChild(new StackPanel { Orientation = Orientation.Horizontal }
            .WithChild(new TextBlock { Text = "For", Margin = new Thickness(0, 0, 6, 0), VerticalAlignment = VerticalAlignment.Center })
            .WithChild(new Control_TimeAndUnit()
                .WithBinding(Control_TimeAndUnit.TimeProperty, new Binding("ExtensionTime") { Source = this, Mode = BindingMode.TwoWay })
                .WithBinding(Control_TimeAndUnit.UnitProperty, new Binding("TimeUnit") { Source = this, Mode = BindingMode.TwoWay })));

    protected override bool IsInvalidatedChild(IGameState gameState)
    {
        if (Evaluatable.IsInvalidated(gameState))
        {
            return true;
        }

        // check for timeout
        var expired = IsTimerTrue();
        return _lastTimerValue != expired;
    }

    protected override bool Execute(IGameState gameState) {
        var res = Evaluatable.EvaluateBool(gameState);
        if (res) _sw.Restart();
        var isExpired = IsTimerTrue();
        _lastTimerValue = isExpired;
        return isExpired;
    }

    private bool IsTimerTrue()
    {
        return TimeUnit switch
        {
            TimeUnit.Milliseconds => _sw.IsRunning && _sw.Elapsed.TotalMilliseconds < ExtensionTime,
            TimeUnit.Seconds => _sw.IsRunning && _sw.Elapsed.TotalSeconds < ExtensionTime,
            TimeUnit.Minutes => _sw.IsRunning && _sw.Elapsed.TotalMinutes < ExtensionTime,
            TimeUnit.Hours => _sw.IsRunning && _sw.Elapsed.TotalHours < ExtensionTime,
            _ => false
        };
    }
        
    public override Evaluatable<bool> Clone() => new BooleanExtender { Evaluatable = Evaluatable.Clone(), ExtensionTime = ExtensionTime, TimeUnit = TimeUnit };
}