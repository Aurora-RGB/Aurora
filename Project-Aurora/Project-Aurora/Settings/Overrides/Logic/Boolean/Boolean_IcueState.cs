using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using AuroraRgb.Modules;
using AuroraRgb.Profiles;
using AuroraRgb.Utils;

namespace AuroraRgb.Settings.Overrides.Logic.Boolean;

[Evaluatable("Icue State", category: EvaluatableCategory.Icue)]
public class BooleanIcueState : Evaluatable<bool>
{
    public Evaluatable<string> StateName { get; set; } = new StringConstant();

    public BooleanIcueState()
    {
    }

    public BooleanIcueState(Evaluatable<string> variableName)
    {
        StateName = variableName;
    }

    public BooleanIcueState(string variableName)
    {
        StateName = new StringConstant
        {
            Value = variableName
        };
    }

    protected override bool Execute(IGameState gameState)
    {
        var state = StateName.Evaluate(gameState);
        return IcueModule.AuroraIcueServer.Gsi.States.Contains(state);
    }

    public override Visual GetControl()
    {
        return new StackPanel { Orientation = Orientation.Horizontal }
            .WithChild(new TextBlock
                { Text = "State", FontWeight = FontWeights.Bold, Margin = new Thickness(2, 0, 6, 0), VerticalAlignment = VerticalAlignment.Center })
            .WithChild(new Control_EvaluatablePresenter { EvalType = typeof(string) }
                .WithBinding(Control_EvaluatablePresenter.ExpressionProperty, new Binding(nameof(StateName)) { Source = this, Mode = BindingMode.TwoWay }));
    }

    public override Evaluatable<bool> Clone()
    {
        return new BooleanIcueState(StateName);
    }
}