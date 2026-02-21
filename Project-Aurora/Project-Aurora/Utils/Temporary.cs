using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Nito.AsyncEx;

namespace AuroraRgb.Utils;

public sealed class Temporary<T>(Func<T> produce, bool callDispose = true) : IDisposable, IAsyncDisposable
    where T : class
{
    public event EventHandler? ValueCreated;

    private readonly Lock _createLock = new();
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

            lock (_createLock)
            {
                // prevent deletions with thread starvation
                using var readLock = TimerLock.WriterLock();

                if (_value != null)
                {
                    return _value;
                }

                var value = produce();
                _value = value;
                ValueCreated?.Invoke(this, EventArgs.Empty);
                AddInstance(this);

                _lastAccess = DateTime.UtcNow;
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
    private static AsyncTimer? _aliveTimer;

    // ReSharper disable once StaticMemberInGenericType
    private static readonly AsyncReaderWriterLock TimerLock = new();

    private static void AddInstance(Temporary<T> temporary)
    {
        Instances.Add(temporary);
        _aliveTimer ??= StartAliveTimer();
    }

    private static AsyncTimer StartAliveTimer()
    {
        return new AsyncTimer(AliveTimerCallback, TimeSpan.FromSeconds(20));
    }

    private static async Task AliveTimerCallback(CancellationToken cancellationToken)
    {
        using var writeLock = await TimerLock.WriterLockAsync(cancellationToken);

        // delay the deletion a bit if threads are starved
        await Task.Yield();

        Instances.RemoveAll(ExpiredInstance);
        if (Instances.Count != 0)
        {
            return;
        }

        if (_aliveTimer != null)
        {
            await _aliveTimer.DisposeAsync();
        }
        _aliveTimer = null;
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