using Common.Data;
using Microsoft.Scripting.Utils;

namespace Common.Utils;

internal static class MemorySharedEventThread
{
    private static readonly List<HandlesAndThread> HandleThreads = [];

    internal static void AddObject(SignaledMemoryObject o)
    {
        lock (HandleThreads)
        {
            var handleThread = HandleThreads.Find(ht => ht.HasSpace(2));
            if (handleThread == null)
            {
                handleThread = new HandlesAndThread();
                HandleThreads.Add(handleThread);
            }
        
            handleThread.AddToThread(o);
        }
    }

    internal static void RemoveObject(SignaledMemoryObject o)
    {
        lock (HandleThreads)
        {
            foreach (var handlesAndThread in HandleThreads)
            {
                handlesAndThread.RemoveIfExists(o);
            }
        }
    }

    private sealed class HandlesAndThread
    {
        private const int MaxHandles = 64;
        
        private readonly SemaphoreSlim _semaphore = new(1);
    
        private CancellationTokenSource _cancellation = new();
        private CancellationTokenSource CancelToken
        {
            get => _cancellation;
            set
            {
                var old = _cancellation;
                _cancellation = value;
                _handles[0] = value.Token.WaitHandle;
                old.Cancel();
                old.Dispose();
            }
        }
        
        private Thread _thread = new(() => { });
        
        private Action[] _actions = [() => { }];
        private WaitHandle[] _handles;

        internal HandlesAndThread()
        {
            _handles = [CancelToken.Token.WaitHandle];
            _thread.Start();
        }

        private Thread CreateThread()
        {
            var thread = new Thread(() =>
            {
                try
                {
                    _semaphore.Wait(CancelToken.Token);
                    ThreadCallback();
                }
                catch (OperationCanceledException)
                {
                    // end the thread
                }
                finally
                {
                    _semaphore.Release();
                }
            })
            {
                Name = "Memory Share Event Thread",
                IsBackground = true,
                Priority = ThreadPriority.Highest,
            };
            thread.Start();
            Thread.Yield();
            return thread;
        }

        private void ThreadCallback(){
            if (_handles.Length <= 1)
            {
                // stop thread if only handle is cancel token
                return;
            }
            while (true)
            {
                if (CancelToken.IsCancellationRequested)
                {
                    return;
                }
                var i = WaitHandle.WaitAny(_handles);
                switch (i)
                {
                    case 0:
                        return;
                    default:
                        if (i >= _handles.Length)
                        {
                            break;
                        }
                        Task.Run(() =>
                        {
                            _actions[i].Invoke();
                        });
                        break;
                }
            }
        }

        internal void AddToThread(SignaledMemoryObject o)
        {
            CancelToken.Cancel();
            _semaphore.Wait();
            CancelToken = new CancellationTokenSource();

            try
            {
                _actions = _actions.Concat([
                    o.OnUpdated,
                    o.OnUpdateRequested
                ]).ToArray();
                _handles = _handles.Concat([
                    o.ObjectUpdatedHandle,
                    o.UpdateRequestedHandle
                ]).ToArray();

                _thread = CreateThread();
            }
            finally
            {
                _semaphore.Release();
            }
        }

        internal void RemoveIfExists(SignaledMemoryObject o)
        {
            CancelToken.Cancel();
            _semaphore.Wait();
            CancelToken = new CancellationTokenSource();

            try
            {
                var updatedHandleIndex = _handles.FindIndex(h => o.ObjectUpdatedHandle == h);
                if (updatedHandleIndex != -1)
                {
                    _actions = _actions.Where((_, i) => i != updatedHandleIndex).ToArray();
                    _handles = _handles.Where((_, i) => i != updatedHandleIndex).ToArray();
                }

                var requestedHandleIndex = _handles.FindIndex(h => o.UpdateRequestedHandle == h);
                if (requestedHandleIndex != -1)
                {
                    _actions = _actions.Where((_, i) => i != requestedHandleIndex).ToArray();
                    _handles = _handles.Where((_, i) => i != requestedHandleIndex).ToArray();
                }

                _thread = CreateThread();
            }
            finally
            {
                _semaphore.Release();
            }
        }

        internal bool HasSpace(int handleCount)
        {
            return _handles.Length + handleCount < MaxHandles && _actions.Length + handleCount < MaxHandles;
        }
    }
}