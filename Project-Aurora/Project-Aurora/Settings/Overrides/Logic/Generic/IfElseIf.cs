using System;
using System.Collections.ObjectModel;
using System.Windows.Media;
using AuroraRgb.Profiles;
using AuroraRgb.Settings.Overrides.Logic.Generic;
using AuroraRgb.Utils;

namespace AuroraRgb.Settings.Overrides.Logic;

public abstract class IfElseGeneric<T> : Evaluatable<T> {
    /// <summary>
    /// A list of all branches of the conditional.
    /// </summary>
    public ObservableCollection<Branch> Cases { get; set; } = CreateDefaultCases(
        new BooleanConstant(), // Condition
        EvaluatableDefaults.Get<T>(), // True
        EvaluatableDefaults.Get<T>() // False
    );

    /// <summary>Creates a new If-Else evaluatable with default evaluatables.</summary>
    public IfElseGeneric() { }
    /// <summary>Creates a new evaluatable that returns caseTrue if condition evaluates to true and caseFalse otherwise.</summary>
    public IfElseGeneric(Evaluatable<bool> condition, Evaluatable<T> caseTrue, Evaluatable<T> caseFalse) : this() { Cases = CreateDefaultCases(condition, caseTrue, caseFalse); }
    /// <summary>Creates a new evaluatable using the given case tree.</summary>
    public IfElseGeneric(ObservableCollection<Branch> cases) : this() { Cases = cases; }

    public override Visual GetControl() => new Control_Ternary<T>(this);

    /// <summary>Evaluate conditions and return the appropriate evaluation.</summary>
    protected override T Execute(IGameState gameState) {
        foreach (var branch in Cases)
            if (branch.Condition == null || branch.Condition.Evaluate(gameState)) // Find the first with a true condition, or where the condition is null (which indicates 'else')
                return branch.Value.Evaluate(gameState);
        return default;
    }

    private static ObservableCollection<Branch> CreateDefaultCases(Evaluatable<bool> condition, Evaluatable<T> caseTrue, Evaluatable<T> caseFalse) =>
    [
        new(condition, caseTrue),
        new(null, caseFalse)
    ];

    public class Branch : ICloneable {
        public Evaluatable<bool> Condition { get; set; }
        public Evaluatable<T> Value { get; set; }

        public Branch(Evaluatable<bool> condition, Evaluatable<T> value) { Condition = condition; Value = value; }

        public object Clone() => new Branch(Condition?.Clone(), Value.Clone());
    }
}


// Concrete classes
[Evaluatable("If - Else If - Else", category: EvaluatableCategory.Logic)]
public class IfElseBoolean : IfElseGeneric<bool> {
    /// <summary>Creates a new If-Else evaluatable with default evaluatables.</summary>
    public IfElseBoolean()
    { }

    /// <summary>Creates a new evaluatable that returns caseTrue if condition evaluates to true and caseFalse otherwise.</summary>
    public IfElseBoolean(Evaluatable<bool> condition, Evaluatable<bool> caseTrue, Evaluatable<bool> caseFalse) : base(condition, caseTrue, caseFalse) { }

    /// <summary>Creates a new evaluatable using the given case tree.</summary>
    public IfElseBoolean(ObservableCollection<Branch> cases) : base(cases) { }
    public override Evaluatable<bool> Clone() => new IfElseBoolean(Cases.Clone());
    protected override bool ExecuteBool(IGameState gameState) => Execute(gameState);
    protected override int ExecuteInt(IGameState gameState)
    {
        throw new InvalidOperationException();
    }
    protected override double ExecuteDouble(IGameState gameState)
    {
        throw new InvalidOperationException();
    }
}


[Evaluatable("If - Else If - Else", category: EvaluatableCategory.Logic)]
public class IfElseNumeric : IfElseGeneric<double> {
    /// <summary>Creates a new If-Else evaluatable with default evaluatables.</summary>
    public IfElseNumeric()
    { }

    /// <summary>Creates a new evaluatable that returns caseTrue if condition evaluates to true and caseFalse otherwise.</summary>
    public IfElseNumeric(Evaluatable<bool> condition, Evaluatable<double> caseTrue, Evaluatable<double> caseFalse) : base(condition, caseTrue, caseFalse) { }

    /// <summary>Creates a new evaluatable using the given case tree.</summary>
    public IfElseNumeric(ObservableCollection<Branch> cases) : base(cases) { }
    public override Evaluatable<double> Clone() => new IfElseNumeric(Cases.Clone());
    protected override bool ExecuteBool(IGameState gameState)
    {
        throw new InvalidOperationException();
    }
    protected override int ExecuteInt(IGameState gameState)
    {
        throw new InvalidOperationException();
    }
    protected override double ExecuteDouble(IGameState gameState) => Execute(gameState);
}


[Evaluatable("If - Else If - Else", category: EvaluatableCategory.Logic)]
public class IfElseString : IfElseGeneric<string> {
    /// <summary>Creates a new If-Else evaluatable with default evaluatables.</summary>
    public IfElseString() { }

    /// <summary>Creates a new evaluatable that returns caseTrue if condition evaluates to true and caseFalse otherwise.</summary>
    public IfElseString(Evaluatable<bool> condition, Evaluatable<string> caseTrue, Evaluatable<string> caseFalse) : base(condition, caseTrue, caseFalse) { }

    /// <summary>Creates a new evaluatable using the given case tree.</summary>
    public IfElseString(ObservableCollection<Branch> cases) : base(cases) { }
    public override Evaluatable<string> Clone() => new IfElseString(Cases.Clone());
    protected override bool ExecuteBool(IGameState gameState)
    {
        throw new InvalidOperationException();
    }
    protected override int ExecuteInt(IGameState gameState)
    {
        throw new InvalidOperationException();
    }
    protected override double ExecuteDouble(IGameState gameState)
    {
        throw new InvalidOperationException();
    }
}