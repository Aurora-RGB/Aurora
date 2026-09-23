using System;
using System.ComponentModel;
using System.Windows.Media;
using AuroraRgb.Profiles;

namespace AuroraRgb.Settings.Overrides.Logic;

/// <summary>
/// Interface that defines a logic operand that can be evaluated into a value. Should also have a Visual control
/// that can be used to edit the operand.
/// </summary>
public interface IEvaluatable {
    /// <summary>The most recent value that was output from the evaluatable.</summary>
    object LastValue { get; }
    
    long LastQuery { get; }
    
    bool EvaluateBool(IGameState gameState);

    double EvaluateDouble(IGameState gameState);

    /// <summary>Should return a control that is bound to this logic element.</summary>
    Visual GetControl();

    /// <summary>Creates a copy of this IEvaluatable.</summary>
    IEvaluatable Clone();
}

public abstract partial class Evaluatable<T> : IEvaluatable, INotifyPropertyChanged {
    protected bool FieldsInvalidated { get; private set; } = true;

    [Newtonsoft.Json.JsonIgnore]
    public long LastQuery { get; protected set; }

    /// <summary>The most recent value that was output from the evaluatable.</summary>
    [Newtonsoft.Json.JsonIgnore]
    public virtual object LastValue { get; protected set; } = default;

    public bool IsInvalidated(IGameState gameState) => IsInvalidatedChild(gameState) || FieldsInvalidated;

    protected virtual bool IsInvalidatedChild(IGameState gameState) => true;

    /**
     * Mark last queried value as invalid, cause IsInvalidated to return false until next execution
     */
    protected void Invalidate()
    {
        FieldsInvalidated = true;
    }

    /// <summary>Should evaluate the operand and return the evaluation result.</summary>
    protected abstract T Execute(IGameState gameState);
    protected abstract bool ExecuteBool(IGameState gameState);
    protected abstract double ExecuteDouble(IGameState gameState);

    /// <summary>Evaluates the result of this evaluatable with the given gamestate and returns the result.</summary>
    // Execute the evaluatable logic, store the latest value and return this value
    public T Evaluate(IGameState gameState)
    {
        if (!IsInvalidated(gameState))
        {
            return (T)LastValue;
        }
        
        var newVal = Execute(gameState);
        LastValue = newVal;
        OnValueCalculated();
        return newVal;
    }

    public bool EvaluateBool(IGameState gameState) => ExecuteBool(gameState);
    public double EvaluateDouble(IGameState gameState) => ExecuteDouble(gameState);

    protected void OnValueCalculated()
    {
        FieldsInvalidated = false;
        LastQuery = DateTimeOffset.Now.ToUnixTimeMilliseconds();
        
        // manually trigger for LastValue overridden subclasses
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(LastValue)));
    }

    /// <summary>Should return a control that is bound to this logic element.</summary>
    public abstract Visual GetControl();

    /// <summary>Creates a copy of this IEvaluatable.</summary>
    public abstract Evaluatable<T> Clone();
    IEvaluatable IEvaluatable.Clone() => Clone();
}

public abstract class BoolEvaluatable : Evaluatable<bool>
{
    private bool _lastValue;

    public override object LastValue
    {
        get => _lastValue;
        protected set => _lastValue = (bool)value;
    }

    protected override bool ExecuteBool(IGameState gameState)
    {
        if (!IsInvalidated(gameState))
        {
            return _lastValue;
        }
 
        _lastValue = Execute(gameState);
        OnValueCalculated();
        return _lastValue;
    }

    protected override double ExecuteDouble(IGameState gameState)
    {
        throw new InvalidOperationException();
    }
}

public abstract class DoubleEvaluatable : Evaluatable<double>
{
    private double _lastValue;
    
    public override object LastValue
    {
        get => Math.Round(_lastValue, 2);
        protected set => _lastValue = (double)value;
    }

    protected override bool ExecuteBool(IGameState gameState)
    {
        throw new InvalidOperationException();
    }
    protected override double ExecuteDouble(IGameState gameState)
    {
        if (!IsInvalidated(gameState))
        {
            return _lastValue;
        }
        
        _lastValue = Execute(gameState);
        OnValueCalculated();
        return _lastValue;
    }
}

public abstract class StringEvaluatable : Evaluatable<string>
{
    protected override bool ExecuteBool(IGameState gameState)
    {
        throw new InvalidOperationException();
    }
    protected override double ExecuteDouble(IGameState gameState)
    {
        throw new InvalidOperationException();
    }
}