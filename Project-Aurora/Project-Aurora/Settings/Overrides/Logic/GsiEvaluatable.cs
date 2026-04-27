using System;
using AuroraRgb.EffectsEngine;
using AuroraRgb.Profiles;

namespace AuroraRgb.Settings.Overrides.Logic;

public abstract class GsiEvaluatable<T> : CachedEvaluatable<T> where T : IEquatable<T>
{
    private VariablePath _variablePath = VariablePath.Empty;

    public VariablePath VariablePath
    {
        get => _variablePath;
        set
        {
            // possible on json deserialize
            // ReSharper disable once ConditionIsAlwaysTrueOrFalseAccordingToNullableAPIContract
            if(value == null)
                return;
            _variablePath = value;
            Invalidate();
        }
    }

    // for backwards compatibility
    // ReSharper disable once UnusedMember.Global
    public VariablePath StatePath
    {
        set => _variablePath = value;
    }
}

public abstract class BoolGsiEvaluatable : GsiEvaluatable<bool>
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

public abstract class DoubleGsiEvaluatable : GsiEvaluatable<double>
{
    private double _lastValue;
    
    public override object LastValue
    {
        get => Math.Round(_lastValue, 2);
        protected set => _lastValue = (double)value;
    }

    protected void SetDoubleValue(double value)
    {
        _lastValue = value;
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