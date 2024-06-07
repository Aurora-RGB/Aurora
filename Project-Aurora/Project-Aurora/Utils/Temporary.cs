using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace AuroraRgb.Utils;

public sealed class Temporary<T>(Func<T> produce) : IDisposable, IAsyncDisposable
    where T : class
{
    private static readonly List<Temporary<T>> Instances = [];
    // ReSharper disable once StaticMemberInGenericType
    private static Timer? _aliveTimer;
    // ReSharper disable once StaticMemberInGenericType
    private static readonly ReaderWriterLockSlim TimerLock = new();

    public event EventHandler? ValueCreated;

    private T? _value;

    private long _lastAccess = Time.GetMillisecondsSinceEpoch();
    private readonly double _inactiveTimeMilliseconds = TimeSpan.FromSeconds(20).TotalMilliseconds;
    private readonly ReaderWriterLockSlim _lock = new(LockRecursionPolicy.SupportsRecursion);

    public T Value
    {
        get
        {
            _lock.EnterUpgradeableReadLock();
            _lastAccess = Time.GetMillisecondsSinceEpoch();

            if (_value != null)
            {
                _lock.ExitUpgradeableReadLock();
                return _value;
            }

            _lock.EnterWriteLock();
            _value = produce.Invoke();
            ValueCreated?.Invoke(this, EventArgs.Empty);
            AddInstance(this);
            _lock.ExitWriteLock();
            _lock.ExitUpgradeableReadLock();
            return _value;
        }
    }

    public bool HasValue => _value != null;

    private static void AddInstance(Temporary<T> temporary)
    {
        TimerLock.EnterWriteLock();
        Instances.Add(temporary);
        _aliveTimer ??= StartAliveTimer();
        TimerLock.ExitWriteLock();
    }

    private static Timer StartAliveTimer()
    {
        return new Timer(AliveTimerCallback, null, TimeSpan.FromSeconds(20), TimeSpan.FromSeconds(20));
    }

    private static void AliveTimerCallback(object? state)
    {
        TimerLock.EnterReadLock();

        Instances.RemoveAll(CheckInstance);
        if (Instances.Count != 0)
        {
            TimerLock.ExitReadLock();
            return;
        }

        _aliveTimer?.Dispose();
        _aliveTimer = null;

        TimerLock.ExitReadLock();
    }

    private static bool CheckInstance(Temporary<T> temporary)
    {
        var now = Time.GetMillisecondsSinceEpoch();
        if (now - temporary._lastAccess <= temporary._inactiveTimeMilliseconds || temporary._value == null) return false;

        // 20 sec passed since last render, dispose proxy
        var temporaryValue = temporary._value;
        temporary._value = null;
        if (temporaryValue is IDisposable disposable)
        {
            disposable.Dispose();
        }

        return true;
    }

    public void Dispose()
    {
        _lock.EnterWriteLock();
        Instances.Remove(this);
        _lock.ExitWriteLock();

        if (_value is IDisposable disposable)
        {
            disposable.Dispose();
        }
        _value = null;
    }

    public async ValueTask DisposeAsync()
    {
        _lock.EnterWriteLock();
        Instances.Remove(this);
        _lock.ExitWriteLock();

        switch (_value)
        {
            case IAsyncDisposable asyncDisposable:
                await asyncDisposable.DisposeAsync();
                break;
            case IDisposable disposable:
                disposable.Dispose();
                break;
        }
    }
}