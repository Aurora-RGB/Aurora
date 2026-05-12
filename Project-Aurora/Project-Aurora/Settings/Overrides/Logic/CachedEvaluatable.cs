using System;
using AuroraRgb.Profiles;

namespace AuroraRgb.Settings.Overrides.Logic;

public abstract class CachedEvaluatable<T> : Evaluatable<T> where T : IEquatable<T>
{
    private T _lastValue;

    public override object LastValue
    {
        get => _lastValue;
        protected set => _lastValue = (T)value;
    }

    protected override bool IsInvalidatedChild(IGameState gameState) => UpdateState(gameState);

    protected override T Execute(IGameState gameState)
    {
        return _lastValue;
    }

    protected abstract T Calculate(IGameState gameState);

    private bool UpdateState(IGameState gameState)
    {
        var value = Calculate(gameState);
        var changed = !value.Equals(_lastValue);
        if (changed)
        {
            _lastValue = value;
            Invalidate();
        }

        LastQuery = DateTimeOffset.Now.ToUnixTimeMilliseconds();
        return changed;
    }
}

public abstract class BoolCachedEvaluatable : CachedEvaluatable<bool>
{
    protected override bool ExecuteBool(IGameState gameState) => Calculate(gameState);

    protected override double ExecuteDouble(IGameState gameState) => throw new InvalidOperationException();
}

public abstract class DoubleCachedEvaluatable : CachedEvaluatable<double>
{
    protected override bool ExecuteBool(IGameState gameState) => throw new InvalidOperationException();

    protected override double ExecuteDouble(IGameState gameState) => Calculate(gameState);
}