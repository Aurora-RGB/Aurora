using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using AuroraRgb.Nodes;
using AuroraRgb.Profiles;
using AuroraRgb.Utils;

namespace AuroraRgb.Settings.Overrides.Logic.Boolean;

[Evaluatable("Global Variable Exists", category: EvaluatableCategory.Global)]
public class Boolean_VariableExists : Evaluatable<bool>
{
    public Evaluatable<string> VariableName { get; set; } = new StringConstant();
    
    public Boolean_VariableExists() { }
    
    public Boolean_VariableExists(Evaluatable<string> variableName) {
        VariableName = variableName;
    }
 
    protected override bool Execute(IGameState gameState)
    {
        var key = VariableName.Evaluate(gameState);
        return AuroraVariables.Instance.Booleans.ContainsKey(key) ||
               AuroraVariables.Instance.Numbers.ContainsKey(key) ||
               AuroraVariables.Instance.Strings.ContainsKey(key);
    }

    public override Visual GetControl()
    {
        return new StackPanel { Orientation = Orientation.Horizontal }
            .WithChild(new TextBlock
                { Text = "Name", FontWeight = FontWeights.Bold, Margin = new Thickness(2, 0, 6, 0), VerticalAlignment = VerticalAlignment.Center })
            .WithChild(new Control_EvaluatablePresenter { EvalType = typeof(string) }
                .WithBinding(Control_EvaluatablePresenter.ExpressionProperty, new Binding(nameof(VariableName)) { Source = this, Mode = BindingMode.TwoWay }));
    }

    public override Evaluatable<bool> Clone()
    {
        return new Boolean_VariableExists(VariableName);
    }
}