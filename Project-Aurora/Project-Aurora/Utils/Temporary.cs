using System;
using System.Threading;
using System.Threading.Tasks;

namespace AuroraRgb.Utils;

public sealed class Temporary<T>(Func<T> produce) : IDisposable, IAsyncDisposable
    where T : class
{
    public event EventHandler? ValueCreated;

    private T? _value;

    private long _lastAccess = Time.GetMillisecondsSinceEpoch();
    private Timer? _aliveTimer;
    private readonly double _inactiveTimeMilliseconds = TimeSpan.FromSeconds(20).TotalMilliseconds;
    private readonly ReaderWriterLockSlim _lock = new(LockRecursionPolicy.SupportsRecursion);

    public T Value
    {
        get
        {
            _lock.EnterUpgradeableReadLock();
            _lastAccess = Time.GetMillisecondsSinceEpoch();
            _aliveTimer ??= StartAliveTimer();

            if (_value != null)
            {
                _lock.ExitUpgradeableReadLock();
                return _value;
            }

            _lock.EnterWriteLock();
            _value = produce.Invoke();
            ValueCreated?.Invoke(this, EventArgs.Empty);
            _lock.ExitWriteLock();
            _lock.ExitUpgradeableReadLock();
            return _value;
        }
    }

    public bool HasValue => _value != null;

    private Timer StartAliveTimer()
    {
        return new Timer(AliveTimerCallback, null, TimeSpan.FromSeconds(20), TimeSpan.FromSeconds(20));
    }

    private void AliveTimerCallback(object? state)
    {
        var now = Time.GetMillisecondsSinceEpoch();
        if (now - _lastAccess <= _inactiveTimeMilliseconds || _value == null) return;

        // 20 sec passed since last render, dispose proxy
        var temporaryValue = _value;
        _value = null;
        if (temporaryValue is IDisposable disposable)
        {
            disposable.Dispose();
        }

        _aliveTimer?.Dispose();
        _aliveTimer = null;
    }

    public void Dispose()
    {
        _aliveTimer?.Dispose();

        if (_value is IDisposable disposable)
        {
            disposable.Dispose();
        }
        _value = null;
    }

    public async ValueTask DisposeAsync()
    {
        if (_aliveTimer != null) await _aliveTimer.DisposeAsync();
        if (_value is IDisposable disposable)
        {
            disposable.Dispose();
        }
    }
}