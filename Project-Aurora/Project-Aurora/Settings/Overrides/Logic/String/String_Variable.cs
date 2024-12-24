using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using AuroraRgb.Nodes;
using AuroraRgb.Profiles;
using AuroraRgb.Utils;

namespace AuroraRgb.Settings.Overrides.Logic;

[Evaluatable("String Global Variable", category: EvaluatableCategory.Global)]
public class String_Variable : Evaluatable<string>
{
    public Evaluatable<string> DefaultValue { get; set; } = new StringConstant();
    public Evaluatable<string> VariableName { get; set; } = new StringConstant();
    
    public String_Variable() { }

    public String_Variable(Evaluatable<string> variableName, Evaluatable<string> defaultValue)
    {
        VariableName = variableName;
        DefaultValue = defaultValue;
    }

    protected override string Execute(IGameState gameState)
    {
        var key = VariableName.Evaluate(gameState);
        var defaultValue = DefaultValue.Evaluate(gameState);
        return AuroraVariables.Instance.Strings.GetValueOrDefault(key, defaultValue);
    }

    public override Visual GetControl()
    {
        return new StackPanel { Orientation = Orientation.Horizontal }
            .WithChild(new TextBlock { Text = "Name", FontWeight = FontWeights.Bold, Margin = new Thickness(2, 0, 6, 0), VerticalAlignment = VerticalAlignment.Center })
            .WithChild(new Control_EvaluatablePresenter { EvalType = typeof(string) }
                .WithBinding(Control_EvaluatablePresenter.ExpressionProperty, new Binding(nameof(VariableName)) { Source = this, Mode = BindingMode.TwoWay }))
            .WithChild(new TextBlock { Text = "Default Value", FontWeight = FontWeights.Bold, Margin = new Thickness(2, 0, 6, 0), VerticalAlignment = VerticalAlignment.Center })
            .WithChild(new Control_EvaluatablePresenter { EvalType = typeof(string) }
                .WithBinding(Control_EvaluatablePresenter.ExpressionProperty, new Binding(nameof(DefaultValue)) { Source = this, Mode = BindingMode.TwoWay }));
    }

    public override Evaluatable<string> Clone()
    {
        return new String_Variable(VariableName, DefaultValue);
    }
}