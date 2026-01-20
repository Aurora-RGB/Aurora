using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using AuroraRgb.Modules;
using AuroraRgb.Profiles;
using AuroraRgb.Utils;

namespace AuroraRgb.Settings.Overrides.Logic.Boolean;

[Evaluatable("Icue Event Triggered", category: EvaluatableCategory.Icue)]
public class BooleanIcueEventTriggered : BoolEvaluatable
{
    public Evaluatable<string> EventName { get; set; } = new StringConstant();
    public Evaluatable<double> TimeoutSeconds { get; set; } = new NumberConstant(2);

    public BooleanIcueEventTriggered()
    {
    }

    public BooleanIcueEventTriggered(Evaluatable<string> eventName, Evaluatable<double> timeoutSeconds)
    {
        EventName = eventName;
        TimeoutSeconds = timeoutSeconds;
    }

    public BooleanIcueEventTriggered(string eventName, double timeoutSeconds)
    {
        EventName = new StringConstant(eventName);
        TimeoutSeconds = new NumberConstant(timeoutSeconds);
    }

    protected override bool Execute(IGameState gameState)
    {
        var eventName = EventName.Evaluate(gameState);
        var timeoutSeconds = TimeoutSeconds.Evaluate(gameState);
        if (string.IsNullOrWhiteSpace(eventName) || timeoutSeconds <= 0)
            return false;
        var timeout = (long)(timeoutSeconds * 1000);
        var currentTimeMillis = System.DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        var lastEventTimeMillis = IcueModule.AuroraIcueServer.Gsi.EventTimestamps.GetValueOrDefault(eventName, 0);
        return currentTimeMillis - lastEventTimeMillis <= timeout;
    }

    public override Visual GetControl()
    {
        return new StackPanel { Orientation = Orientation.Horizontal }
            .WithChild(new TextBlock
                { Text = "Event", FontWeight = FontWeights.Bold, Margin = new Thickness(2, 0, 6, 0), VerticalAlignment = VerticalAlignment.Center })
            .WithChild(new Control_EvaluatablePresenter { EvalType = typeof(string) }
                .WithBinding(Control_EvaluatablePresenter.ExpressionProperty, new Binding(nameof(EventName)) { Source = this, Mode = BindingMode.TwoWay }))
            .WithChild(new TextBlock
                { Text = "Duration (seconds)", FontWeight = FontWeights.Bold, Margin = new Thickness(2, 0, 6, 0), VerticalAlignment = VerticalAlignment.Center })
            .WithChild(new Control_EvaluatablePresenter { EvalType = typeof(double) }
                .WithBinding(Control_EvaluatablePresenter.ExpressionProperty, new Binding(nameof(TimeoutSeconds)) { Source = this, Mode = BindingMode.TwoWay }));
    }

    public override Evaluatable<bool> Clone()
    {
        return new BooleanIcueEventTriggered(EventName, TimeoutSeconds);
    }
}