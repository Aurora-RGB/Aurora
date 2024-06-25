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

    private long _lastAccess = Time.GetMillisecondsSinceEpoch();
    private readonly double _inactiveTimeMilliseconds = TimeSpan.FromSeconds(20).TotalMilliseconds;
    private readonly bool _callDispose = callDispose;

    public T Value
    {
        get
        {
            _lastAccess = Time.GetMillisecondsSinceEpoch();

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
        TimerLock.EnterReadLock();

        Instances.RemoveAll(ExpiredInstance);
        if (Instances.Count != 0)
        {
            TimerLock.ExitReadLock();
            return;
        }

        _aliveTimer?.Dispose();
        _aliveTimer = null;

        TimerLock.ExitReadLock();
    }

    private static bool ExpiredInstance(Temporary<T> temporary)
    {
        var now = Time.GetMillisecondsSinceEpoch();
        if (now - temporary._lastAccess <= temporary._inactiveTimeMilliseconds || temporary._value == null) return false;

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