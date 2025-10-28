using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using AuroraRgb.Modules;
using AuroraRgb.Profiles;
using AuroraRgb.Utils;

namespace AuroraRgb.Settings.Overrides.Logic.Number;

[Evaluatable("Icue Event Fade", category: EvaluatableCategory.Icue)]
public class NumberIcueEventFade : Evaluatable<double>
{
    public Evaluatable<string> EventName { get; set; } = new StringConstant();
    public Evaluatable<double> TimeoutSeconds { get; set; } = new NumberConstant(2);

    public NumberIcueEventFade()
    {
    }

    public NumberIcueEventFade(Evaluatable<string> eventName, Evaluatable<double> timeoutSeconds)
    {
        EventName = eventName;
        TimeoutSeconds = timeoutSeconds;
    }

    public NumberIcueEventFade(string eventName, int timeoutSeconds)
    {
        EventName = new StringConstant
        {
            Value = eventName
        };
        TimeoutSeconds = new NumberConstant(timeoutSeconds);
    }

    protected override double Execute(IGameState gameState)
    {
        var eventName = EventName.Evaluate(gameState);
        var timeoutSeconds = TimeoutSeconds.Evaluate(gameState);
        if (string.IsNullOrWhiteSpace(eventName) || timeoutSeconds <= 0)
            return 0.0;
        var timeout = (long)(timeoutSeconds * 1000);
        var currentTimeMillis = System.DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        var lastEventTimeMillis = IcueModule.AuroraIcueServer.Gsi.EventTimestamps.GetValueOrDefault(eventName, 0);
        var timeSinceEvent = currentTimeMillis - lastEventTimeMillis;
        if (timeSinceEvent > timeout)
            return 0.0;
        return 1.0 - (double)timeSinceEvent / timeout;
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

    public override Evaluatable<double> Clone()
    {
        return new NumberIcueEventFade(EventName, TimeoutSeconds);
    }
}