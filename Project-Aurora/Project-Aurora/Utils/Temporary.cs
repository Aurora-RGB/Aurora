using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace AuroraRgb.Utils;

public sealed class Temporary<T>(Func<T> produce, bool callDispose = true) : IDisposable, IAsyncDisposable
    where T : class
{
    public event EventHandler? ValueCreated;

    private readonly object _createLock = new();
    private volatile T? _value;

    private DateTime _lastAccess = DateTime.UtcNow;
    private readonly TimeSpan _inactiveTimeSpan = TimeSpan.FromSeconds(20);
    private readonly bool _callDispose = callDispose;

    public T Value
    {
        get
        {
            _lastAccess = DateTime.UtcNow;

            if (_value != null)
            {
                return _value;
            }

            lock(_createLock)
            {
                if (_value != null)
                {
                    return _value;
                }

                //TODO add an infinite loop check
                var value = produce.Invoke();
                _value = value;
                ValueCreated?.Invoke(this, EventArgs.Empty);
                AddInstance(this);
                return value;
            }
        }
    }

    public bool HasValue => _value != null;

    public void Dispose()
    {
        lock (Instances)
        {
            Instances.Remove(this);
        }

        if (!_callDispose)
        {
            return;
        }

        if (_value is IDisposable disposable)
        {
            disposable.Dispose();
        }
        _value = null;
    }

    public async ValueTask DisposeAsync()
    {
        lock (Instances)
        {
            Instances.Remove(this);
        }

        if (!_callDispose)
        {
            return;
        }

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

    #region static

    private static readonly List<Temporary<T>> Instances = [];
    // ReSharper disable once StaticMemberInGenericType
    private static Timer? _aliveTimer;
    // ReSharper disable once StaticMemberInGenericType
    private static readonly ReaderWriterLockSlim TimerLock = new();

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
        TimerLock.EnterWriteLock();

        Instances.RemoveAll(ExpiredInstance);
        if (Instances.Count != 0)
        {
            TimerLock.ExitWriteLock();
            return;
        }

        _aliveTimer?.Dispose();
        _aliveTimer = null;

        TimerLock.ExitWriteLock();
    }

    private static bool ExpiredInstance(Temporary<T> temporary)
    {
        var now = DateTime.UtcNow;
        if (now - temporary._lastAccess <= temporary._inactiveTimeSpan || temporary._value == null) return false;

        var temporaryValue = temporary._value;
        temporary._value = null;
        if (temporary._callDispose && temporaryValue is IDisposable disposable)
        {
            disposable.Dispose();
        }

        return true;
    }

    #endregion
}