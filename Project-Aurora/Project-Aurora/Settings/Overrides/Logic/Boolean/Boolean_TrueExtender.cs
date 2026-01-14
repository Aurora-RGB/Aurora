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

    protected override bool Execute(IGameState gameState) {
        var res = Evaluatable.Evaluate(gameState);
        if (res) _sw.Restart();
        switch (TimeUnit) {
            case TimeUnit.Milliseconds: return _sw.IsRunning && _sw.Elapsed.TotalMilliseconds < ExtensionTime;
            case TimeUnit.Seconds: return _sw.IsRunning && _sw.Elapsed.TotalSeconds < ExtensionTime;
            case TimeUnit.Minutes: return _sw.IsRunning && _sw.Elapsed.TotalMinutes < ExtensionTime;
            case TimeUnit.Hours: return _sw.IsRunning && _sw.Elapsed.TotalHours < ExtensionTime;
            default: return false;
        }
    }
        
    public override Evaluatable<bool> Clone() => new BooleanExtender { Evaluatable = Evaluatable.Clone(), ExtensionTime = ExtensionTime, TimeUnit = TimeUnit };
}