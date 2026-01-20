using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using AuroraRgb.Profiles;
using AuroraRgb.Utils;

namespace AuroraRgb.Settings.Overrides.Logic;

/// <summary>
/// Represents a constant string value that will always evaluate to the same value.
/// </summary>
[Evaluatable("String Constant", category: EvaluatableCategory.String)]
public class StringConstant : StringEvaluatable {

    /// <summary>The value of the constant.</summary>
    public string Value { get; set; } = "";

    public StringConstant()
    {
    }

    public StringConstant(string value)
    {
        Value = value;
    }

    /// <summary>A control for setting the string value</summary>
    public override Visual GetControl() => new TextBox { MinWidth = 40, VerticalAlignment = System.Windows.VerticalAlignment.Center }
        .WithBinding(TextBox.TextProperty, new Binding("Value") { Source = this, Mode = BindingMode.TwoWay });

    /// <summary>Simply return the constant value.</summary>
    protected override string Execute(IGameState gameState) => Value;
                
    /// <summary>Clones this constant string value.</summary>
    public override Evaluatable<string> Clone() => new StringConstant { Value = Value };
}