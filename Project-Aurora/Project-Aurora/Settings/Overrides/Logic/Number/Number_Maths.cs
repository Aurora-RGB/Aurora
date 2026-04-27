using System;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using AuroraRgb.Profiles;
using AuroraRgb.Settings.Overrides.Logic.Number;
using AuroraRgb.Utils;
using Common.Utils;

namespace AuroraRgb.Settings.Overrides.Logic;

/// <summary>
/// Evaluatable that performs a binary mathematical operation on two operands.
/// </summary>
[Evaluatable("Arithmetic Operation", category: EvaluatableCategory.Maths)]
public class NumberMathsOperation : DoubleEvaluatable
{

    /// <summary>Creates a new maths operation that has no values pre-set.</summary>
    public NumberMathsOperation() { }
    /// <summary>Creates a new evaluatable that returns the result of the two given numbers added together.</summary>
    public NumberMathsOperation(double value1, double value2) { Operand1 = new NumberConstant(value1); Operand2 = new NumberConstant(value2); }
    /// <summary>Creates a new evaluatable that returns the result of the two given numbers with the given operator.</summary>
    public NumberMathsOperation(double value1, MathsOperator op, double value2) { Operand1 = new NumberConstant(value1); Operand2 = new NumberConstant(value2); Operator = op; }
    /// <summary>Creates a new evaluatable that returns the result of the given evaluatable and given number added together.</summary>
    public NumberMathsOperation(Evaluatable<double> eval, double value) { Operand1 = eval; Operand2 = new NumberConstant(value); }
    /// <summary>Creates a new evaluatable that returns the result of the given evaluatable and given number with the given operator.</summary>
    public NumberMathsOperation(Evaluatable<double> eval, MathsOperator op, double value) { Operand1 = eval; Operand2 = new NumberConstant(value); Operator = op; }
    /// <summary>Creates a new evaluatable that returns the result of the two given evaluatables added together.</summary>
    public NumberMathsOperation(Evaluatable<double> eval1, Evaluatable<double> eval2) { Operand1 = eval1; Operand2 = eval2; }
    /// <summary>Creates a new evaluatable that returns the result of the two given evaluatables with the given operator.</summary>
    public NumberMathsOperation(Evaluatable<double> eval1, MathsOperator op, Evaluatable<double> eval2) { Operand1 = eval1; Operand2 = eval2; Operator = op; }

    // The operands and the operator
    public Evaluatable<double> Operand1 { get; set; } = new NumberConstant();
    public Evaluatable<double> Operand2 { get; set; } = new NumberConstant();

    public MathsOperator Operator
    {
        get;
        set
        {
            Invalidate();
            field = value;
        }
    } = MathsOperator.Add;

    public override Visual GetControl() => new Control_BinaryOperationHolder(typeof(double), typeof(MathsOperator))
        .WithBinding(Control_BinaryOperationHolder.Operand1Property, new Binding("Operand1") { Source = this, Mode = BindingMode.TwoWay })
        .WithBinding(Control_BinaryOperationHolder.Operand2Property, new Binding("Operand2") { Source = this, Mode = BindingMode.TwoWay })
        .WithBinding(Control_BinaryOperationHolder.SelectedOperatorProperty, new Binding("Operator") { Source = this, Mode = BindingMode.TwoWay });

    protected override bool IsInvalidatedChild(IGameState gameState) => Operand1.IsInvalidated(gameState) || Operand2.IsInvalidated(gameState);

    /// <summary>Resolves the two operands and then compares them using the user specified operator</summary>
    protected override double Execute(IGameState gameState) {
        var op1 = Operand1.EvaluateDouble(gameState);
        var op2 = Operand2.EvaluateDouble(gameState);
        switch (Operator) {
            case MathsOperator.Add: return op1 + op2;
            case MathsOperator.Sub: return op1 - op2;
            case MathsOperator.Mul: return op1 * op2;
            case MathsOperator.Div when op2 != 0: return op1 / op2; // Return 0 if user tried to divide by zero.
            case MathsOperator.Mod when op2 != 0: return op1 % op2;
            default: return 0;
        }
    }
        
    /// <summary>Creates a copy of this maths operation.</summary>
    public override Evaluatable<double> Clone() => new NumberMathsOperation { Operand1 = Operand1.Clone(), Operand2 = Operand2.Clone(), Operator = Operator };
}

/// <summary>
/// Returns the absolute value of the given evaluatable.
/// </summary>
[Evaluatable("Absolute", category: EvaluatableCategory.Maths)]
public class NumberAbsValue : DoubleEvaluatable
{
    /// <summary>Creates a new absolute operation with the default operand.</summary>
    public NumberAbsValue()
    {
    }

    /// <summary>Creates a new absolute evaluatable with the given operand.</summary>
    public NumberAbsValue(Evaluatable<double> op)
    {
        Operand = op;
    }

    /// <summary>The operand to absolute.</summary>
    public Evaluatable<double> Operand { get; set; } = new NumberConstant();

    // Get the control allowing the user to set the operand
    public override Visual GetControl() => new Control_NumericUnaryOpHolder("Absolute")
        .WithBinding(Control_NumericUnaryOpHolder.OperandProperty, new Binding("Operand") { Source = this, Mode = BindingMode.TwoWay });

    protected override bool IsInvalidatedChild(IGameState gameState) => Operand.IsInvalidated(gameState);

    /// <summary>Evaluate the operand and return the absolute value of it.</summary>
    protected override double Execute(IGameState gameState) => Math.Abs(Operand.EvaluateDouble(gameState));

    public override Evaluatable<double> Clone() => new NumberAbsValue { Operand = Operand.Clone() };
}


/// <summary>
/// Evaluatable that compares two numerical evaluatables and returns a boolean depending on the comparison.
/// </summary>
[Evaluatable("Arithmetic Comparison", category: EvaluatableCategory.Maths)]
public class BooleanMathsComparison : BoolEvaluatable {

    /// <summary>Creates a new maths comparison that has no values pre-set.</summary>
    public BooleanMathsComparison() { }
    /// <summary>Creates a new evaluatable that returns whether or not the two given numbers are equal.</summary>
    public BooleanMathsComparison(double value1, double value2) { Operand1 = new NumberConstant(value1); Operand2 = new NumberConstant(value2); }
    /// <summary>Creates a new evaluatable that returns the result of the two given numbers compared using the given operator.</summary>
    public BooleanMathsComparison(double value1, ComparisonOperator op, double value2) { Operand1 = new NumberConstant(value1); Operand2 = new NumberConstant(value2); Operator = op; }
    /// <summary>Creates a new evaluatable that returns whether or not the given evaluatable and given number are equal.</summary>
    public BooleanMathsComparison(Evaluatable<double> eval, double value) { Operand1 = eval; Operand2 = new NumberConstant(value); }
    /// <summary>Creates a new evaluatable that returns the result of the given evaluatable and given number when compared using the given operator.</summary>
    public BooleanMathsComparison(Evaluatable<double> eval, ComparisonOperator op, double value) { Operand1 = eval; Operand2 = new NumberConstant(value); Operator = op; }
    /// <summary>Creates a new evaluatable that returns the whether or not the two given evaluatables are equal.</summary>
    public BooleanMathsComparison(Evaluatable<double> eval1, Evaluatable<double> eval2) { Operand1 = eval1; Operand2 = eval2; }
    /// <summary>Creates a new evaluatable that returns the result of the two given evaluatables when compared using the given operator.</summary>
    public BooleanMathsComparison(Evaluatable<double> eval1, ComparisonOperator op, Evaluatable<double> eval2) { Operand1 = eval1; Operand2 = eval2; Operator = op; }

    // The operands and the operator
    public Evaluatable<double> Operand1 { get; set; } = new NumberConstant();
    public Evaluatable<double> Operand2 { get; set; } = new NumberConstant();

    public ComparisonOperator Operator
    {
        get;
        set
        {
            Invalidate();
            field = value;
        }
    } = ComparisonOperator.EQ;

    // The control allowing the user to edit the evaluatable
    public override Visual GetControl() => new Control_BinaryOperationHolder(typeof(double), typeof(ComparisonOperator))
        .WithBinding(Control_BinaryOperationHolder.Operand1Property, new Binding("Operand1") { Source = this, Mode = BindingMode.TwoWay })
        .WithBinding(Control_BinaryOperationHolder.Operand2Property, new Binding("Operand2") { Source = this, Mode = BindingMode.TwoWay })
        .WithBinding(Control_BinaryOperationHolder.SelectedOperatorProperty, new Binding("Operator") { Source = this, Mode = BindingMode.TwoWay });

    protected override bool IsInvalidatedChild(IGameState gameState) => Operand1.IsInvalidated(gameState) || Operand2.IsInvalidated(gameState);

    /// <summary>Resolves the two operands and then compares them with the user-specified operator.</summary>
    protected override bool Execute(IGameState gameState) {
        var op1 = Operand1.EvaluateDouble(gameState);
        var op2 = Operand2.EvaluateDouble(gameState);
        switch (Operator) {
            case ComparisonOperator.EQ: return Math.Abs(op1 - op2) < 0.00001;
            case ComparisonOperator.NEQ: return Math.Abs(op1 - op2) > 0.00001;
            case ComparisonOperator.GT: return op1 > op2;
            case ComparisonOperator.GTE: return op1 >= op2;
            case ComparisonOperator.LT: return op1 < op2;
            case ComparisonOperator.LTE: return op1 <= op2;
            default: return false;
        }
    }

    /// <summary>Creates a copy of this mathematical comparison.</summary>
    public override Evaluatable<bool> Clone() => new BooleanMathsComparison { Operand1 = Operand1.Clone(), Operand2 = Operand2.Clone(), Operator = Operator };
}



/// <summary>
/// Evaluatable that takes a number in a given range and linearly interpolates it onto another range.
/// </summary>
[Evaluatable("Lerp", category: EvaluatableCategory.Maths)]
public class NumberMap : DoubleEvaluatable {

    /// <summary>Creates a new numeric map with the default constant parameters.</summary>
    public NumberMap() { }
    /// <summary>Creates a new numeric map to map the given value with the given constant range onto the range 0 → 1.</summary>
    public NumberMap(Evaluatable<double> value, double fromMin, double fromMax) : this(value, new NumberConstant(fromMin), new NumberConstant(fromMax)) { }
    /// <summary>Creates a new numeric map to map the given value with the given dynamic range onto the range 0 → 1.</summary>
    public NumberMap(Evaluatable<double> value, Evaluatable<double> fromMin, Evaluatable<double> fromMax) { Value = value; FromMin = fromMin; FromMax = fromMax; }
    /// <summary>Creates a new numeric map to map the given value with the given constant from range onto the given constant to range.</summary>
    public NumberMap(Evaluatable<double> value, double fromMin, double fromMax, double toMin, double toMax) : this(value, new NumberConstant(fromMin), new NumberConstant(fromMax), new NumberConstant(toMin), new NumberConstant(toMax)) { }
    /// <summary>Creates a new numeric map to map the given value with the given dynamic from range onto the given constant to range.</summary>
    public NumberMap(Evaluatable<double> value, Evaluatable<double> fromMin, Evaluatable<double> fromMax, double toMin, double toMax) : this(value, fromMin, fromMax, new NumberConstant(toMin), new NumberConstant(toMax)) { }
    /// <summary>Creates a new numeric map to map the given value with the given dynamic from range onto the given dynamic to range.</summary>
    public NumberMap(Evaluatable<double> value, Evaluatable<double> fromMin, Evaluatable<double> fromMax, Evaluatable<double> toMin, Evaluatable<double> toMax) { Value = value; FromMin = fromMin; FromMax = fromMax; ToMin = toMin; ToMax = toMax; }

    // The value to run through the map
    public Evaluatable<double> Value { get; set; } = new NumberConstant(25);
    // The values representing the starting range of the map
    public Evaluatable<double> FromMin { get; set; } = new NumberConstant(0);
    public Evaluatable<double> FromMax { get; set; } = new NumberConstant(100);
    // The values representing the end range of the map
    public Evaluatable<double> ToMin { get; set; } = new NumberConstant(0);
    public Evaluatable<double> ToMax { get; set; } = new NumberConstant(1);

    // The control to edit the map parameters
    public override Visual GetControl() => new Control_NumericMap(this);

    protected override bool IsInvalidatedChild(IGameState gameState) => Value.IsInvalidated(gameState) || FromMin.IsInvalidated(gameState) || FromMax.IsInvalidated(gameState)
                                                                || ToMin.IsInvalidated(gameState) || ToMax.IsInvalidated(gameState);

    /// <summary>Evaluate the from range and to range and return the value in the new range.</summary>
    protected override double Execute(IGameState gameState) {
        // Evaluate all components
        var value = Value.EvaluateDouble(gameState);
        double fromMin = FromMin.EvaluateDouble(gameState), fromMax = FromMax.EvaluateDouble(gameState);
        double toMin = ToMin.EvaluateDouble(gameState), toMax = ToMax.EvaluateDouble(gameState);

        // Perform actual equation
        return MathUtils.Clamp((value - fromMin) * ((toMax - toMin) / (fromMax - fromMin)) + toMin, Math.Min(toMin, toMax), Math.Max(toMin, toMax));
        // Here is an example of it running: https://www.desmos.com/calculator/nzbiiz7vxv
    }

    public override Evaluatable<double> Clone() => new NumberMap {
        Value = Value.Clone(),
        FromMin = FromMin.Clone(), FromMax = FromMax.Clone(),
        ToMin = ToMin.Clone(), ToMax = ToMax.Clone()
    };
}

[Evaluatable("Random Number", category: EvaluatableCategory.Maths)]
public class NumberRandom : DoubleEvaluatable
{
    private readonly Random _random = new();

    private double _value;

    public Evaluatable<bool> Condition { get; set; } = new BooleanConstant(true);

    public Evaluatable<double> Maximum { get; set; } = new NumberConstant(0);

    public Evaluatable<double> Minimum { get; set; } = new NumberConstant(1);

    public override Visual GetControl() => new StackPanel { Orientation = Orientation.Vertical }
        .WithChild(new TextBlock { Text = "Randomize:" })
        .WithChild(new Control_EvaluatablePresenter { EvalType = typeof(bool) }
            .WithBinding(Control_EvaluatablePresenter.ExpressionProperty, new Binding(nameof(Condition)) { Source = this, Mode = BindingMode.TwoWay }))
        .WithChild(new TextBlock { Text = "Maximum:"})
        .WithChild(new Control_EvaluatablePresenter { EvalType = typeof(double)}
            .WithBinding(Control_EvaluatablePresenter.ExpressionProperty, new Binding(nameof(Minimum)) { Source = this , Mode = BindingMode.TwoWay }))
        .WithChild(new TextBlock { Text = "Minimum:" })
        .WithChild(new Control_EvaluatablePresenter { EvalType = typeof(double) }
            .WithBinding(Control_EvaluatablePresenter.ExpressionProperty, new Binding(nameof(Maximum)) { Source = this, Mode = BindingMode.TwoWay }));

    protected override double Execute(IGameState gameState) {
        var min = Minimum.EvaluateDouble(gameState);
        var max = Maximum.EvaluateDouble(gameState);
        if (Condition.EvaluateBool(gameState))
            _value = _random.NextDouble() * (max - min) + min;

        return _value;
    }

    public override Evaluatable<double> Clone() => new NumberRandom { Minimum = Minimum.Clone(), Maximum = Maximum.Clone(), Condition = Condition.Clone() };
}