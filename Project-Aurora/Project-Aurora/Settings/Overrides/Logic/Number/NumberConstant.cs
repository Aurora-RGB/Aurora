using System.Windows;
using System.Windows.Data;
using System.Windows.Media;
using AuroraRgb.Profiles;
using AuroraRgb.Utils;
using Xceed.Wpf.Toolkit;

namespace AuroraRgb.Settings.Overrides.Logic;

/// <summary>
/// Evaluatable that resolves to a numerical constant.
/// </summary>
[Evaluatable("Number Constant", category: EvaluatableCategory.Maths)]
public class NumberConstant : DoubleEvaluatable
{
    /// <summary>Creates a new constant with the zero as the constant value.</summary>
    public NumberConstant() { }
    /// <summary>Creats a new constant with the given value as the constant value.</summary>
    public NumberConstant(double value) { Value = value; }

    /// <summary>The value of the constant.</summary>
    public double Value
    {
        get;
        set
        {
            Invalidate();
            field = value;
        }
    }

    // The control allowing the user to edit the number value
    public override Visual GetControl() => new DoubleUpDown { VerticalAlignment = VerticalAlignment.Center }
        .WithBinding(DoubleUpDown.ValueProperty, new Binding("Value") { Source = this });

    protected override bool IsInvalidatedChild(IGameState gameState) => false; // A constant is never invalidated, it always returns the same value

    /// <summary>Simply returns the constant value specified by the user</summary>
    protected override double Execute(IGameState gameState) => Value;

    public override Evaluatable<double> Clone() => new NumberConstant { Value = Value };
}