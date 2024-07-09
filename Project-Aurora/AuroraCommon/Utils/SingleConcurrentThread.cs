using Amib.Threading;

namespace Common.Utils;

/**
 * Makes sure only one thread is running the Action.
 * Makes sure last trigger is invoked after the call
 */
public sealed class SingleConcurrentThread
{
    private const bool UsePool = true;

    private readonly SmartThreadPool _worker = new(1000, 1)
    {
        Concurrency = 1,
        MaxQueueLength = 5
    };

    private readonly CancellationTokenSource _cancellationTokenSource = new();
    private readonly SemaphoreSlim _semaphore = new(1, 2);
    private readonly Thread _thread;

    private readonly Action _updateAction;

    public SingleConcurrentThread(string threadName, Action updateAction)
    {
        _updateAction = updateAction;
        _worker.Name = threadName;

        _thread = new Thread(() =>
        {
            while (true)
            {
                _semaphore.Wait();
                if (_cancellationTokenSource.IsCancellationRequested)
                {
                    return;
                }

                _updateAction.Invoke();
            }
        })
        {
          Name  = threadName
        };
        if (!UsePool)
        {
            _thread.Start();
        }
    }

    public void Trigger()
    {
        if (UsePool)
        {
            TriggerPool();
        }
        else
        {
            TriggerThread();
        }
    }

    private void TriggerPool()
    {
        if (_worker.IsShuttingdown)
        {
            return;
        }
        // (_worker.CurrentWorkItemsCount == 0 || _worker.InUseThreads == 0) part wakes the worker when program freezes more than 1 sec
        if (_worker.WaitingCallbacks <= 1 && (_worker.CurrentWorkItemsCount <= 1 || _worker.InUseThreads == 0))
        {
            _worker.QueueWorkItem(_updateAction);
        }
        else if (_worker.IsIdle || _worker.ActiveThreads == 0)
        {
            _worker.Start();
        }
    }

    private void TriggerThread()
    {
        if (_semaphore.CurrentCount > 0)
        {
            return;
        }

        _semaphore.Release();
    }

    public void Dispose(int timeout)
    {
        if (UsePool)
        {
            _worker.Shutdown(timeout);
        }
        else
        {
            _cancellationTokenSource.Cancel();
            _semaphore.Release();
            _thread.Join(timeout);
            _cancellationTokenSource.Dispose();
        }
        _worker.Dispose();
    }
}