using System;
using System.Globalization;
using System.Windows.Media;
using AuroraRgb.EffectsEngine;
using AuroraRgb.Profiles;
using AuroraRgb.Utils.Json;
using Newtonsoft.Json;

namespace AuroraRgb.Settings.Overrides.Logic;

/// <summary>
/// Condition that accesses a specific game state variable (of boolean type) and returns the state.
/// </summary>
[Evaluatable("Boolean State Variable", category: EvaluatableCategory.State)]
public class BooleanGSIBoolean : BoolGsiEvaluatable
{

    /// <summary>Creates an empty boolean state variable lookup.</summary>
    public BooleanGSIBoolean()
    {
    }

    /// <summary>Creates a evaluatable that returns the boolean variable at the given path.</summary>
    public BooleanGSIBoolean(string variablePath)
    {
        VariablePath = new VariablePath(variablePath);
    }

    /// <summary>The control assigned to this condition. Stored as a reference
    /// so that the application be updated if required.</summary>
    [JsonIgnore]
    private Control_ConditionGSIBoolean? _control;
    public override Visual GetControl() => _control ??= new Control_ConditionGSIBoolean(this);

    protected override bool Calculate(IGameState gameState) => gameState.GetBool(VariablePath);

    public override Evaluatable<bool> Clone() => new BooleanGSIBoolean { VariablePath = VariablePath };
}


/// <summary>
/// Condition that accesses some specified game state variables (of numeric type) and returns a comparison between them.
/// </summary>
[Evaluatable("Numeric State Variable", category: EvaluatableCategory.State)]
public class BooleanGSINumeric : BoolGsiEvaluatable {

    // Path to the two GSI variables (or numbers themselves) and the operator to compare them with
    public VariablePath Operand1Path
    {
        get;
        set
        {
            Invalidate();
            field = value;
        }
    } = VariablePath.Empty;

    public VariablePath Operand2Path
    {
        get;
        set
        {
            Invalidate();
            field = value;
        }
    } = VariablePath.Empty;

    public ComparisonOperator Operator
    {
        get;
        set
        {
            Invalidate();
            field = value;
        }
    } = ComparisonOperator.EQ;

    /// <summary>Creates a blank numeric game state lookup evaluatable.</summary>
    public BooleanGSINumeric() { }

    /// <summary>Creates a numeric game state lookup that returns true when the variable at the given path equals the given value.</summary>
    public BooleanGSINumeric(string path1, double val)
    {
        Operand1Path = new VariablePath(path1);
        Operand2Path = new VariablePath(val.ToString());
    }

    /// <summary>Creates a numeric game state lookup that returns true when the variable at path1 equals the given variable at path2.</summary>
    public BooleanGSINumeric(string path1, string path2)
    {
        Operand1Path = new VariablePath(path1);
        Operand2Path = new VariablePath(path2);
    }

    /// <summary>Creates a numeric game state lookup that returns a boolean depending on the given operator's comparison between the variable at the given path and the value.</summary>
    public BooleanGSINumeric(string path1, ComparisonOperator op, double val)
    {
        Operand1Path = new VariablePath(path1);
        Operand2Path = new VariablePath(val.ToString(CultureInfo.InvariantCulture));
        Operator = op;
    }

    /// <summary>Creates a numeric game state lookup that returns a boolean depending on the given operator's comparison between the variable at path1 and the variable at path2.</summary>
    public BooleanGSINumeric(string path1, ComparisonOperator op, string path2)
    {
        Operand1Path = new VariablePath(path1);
        Operand2Path = new VariablePath(path2);
        Operator = op;
    }

    // Control assigned to this condition
    [JsonIgnore]
    private Control_ConditionGSINumeric? _control;
    public override Visual GetControl() => _control ??= new Control_ConditionGSINumeric(this);

    protected override bool Calculate(IGameState gameState) {
        // Parse the operands (either as numbers or paths)
        var op1 = gameState.GetNumber(Operand1Path);
        var op2 = gameState.GetNumber(Operand2Path);

        // Evaluate the operands based on the selected operator and return the result.
        switch (Operator) {
            case ComparisonOperator.EQ: return Math.Abs(op1 - op2) < 0.00001;
            case ComparisonOperator.NEQ: return Math.Abs(op1 - op2) > 0.00001;
            case ComparisonOperator.LT: return op1 < op2;
            case ComparisonOperator.LTE: return op1 <= op2;
            case ComparisonOperator.GT: return op1 > op2;
            case ComparisonOperator.GTE: return op1 >= op2;
            default: return false;
        }
    }
        
    public override Evaluatable<bool> Clone() => new BooleanGSINumeric { Operand1Path = Operand1Path, Operand2Path = Operand2Path, Operator = Operator };
}


/// <summary>
/// Condition that accesses a specified game state variable (of any enum type) and returns a comparison between it and a static enum of the same type.
/// </summary>
[Evaluatable("Enum State Variable", category: EvaluatableCategory.State)]
public class BooleanGSIEnum : BoolGsiEvaluatable {

    /// <summary>Creates a blank enum game state lookup evaluatable.</summary>
    public BooleanGSIEnum()
    {
    }

    /// <summary>Creates an enum game state lookup that returns true when the variable at the given path equals the given enum.</summary>
    public BooleanGSIEnum(string path, Enum val)
    {
        VariablePath = new VariablePath(path);
        EnumValue = val;
    }

    // The value to compare the GSI enum against.
    [JsonConverter(typeof(EnumConverter))]
    public Enum EnumValue
    {
        get;
        set
        {
            Invalidate();
            field = value;
        }
    }

    // Control
    private Control_BooleanGSIEnum? _control;
    public override Visual GetControl() => _control ??= new Control_BooleanGSIEnum(this);

    protected override bool Calculate(IGameState gameState){
        var @enum = gameState.GetEnum(VariablePath);
        return @enum != null && @enum.Equals(EnumValue);
    }

    public override Evaluatable<bool> Clone() => new BooleanGSIEnum { VariablePath = VariablePath, EnumValue = EnumValue };
}