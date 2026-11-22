using System.Security.AccessControl;
using System.Security.Principal;
using Common.Utils;

namespace Common.Data;

public class SignaledMemoryObject : IDisposable
{
    public event EventHandler? Updated;
    public event EventHandler? UpdateRequested;

    internal EventWaitHandle ObjectUpdatedHandle { get; }
    internal EventWaitHandle UpdateRequestedHandle { get; }

    protected bool Disposed;

    /// <summary>
    /// Whether this side waits for the "-updated" event (the readers do). The events are named kernel objects shared by every
    /// process that opens the same memory and they are auto-reset: a Set wakes ONE waiter. A writer that also waited for
    /// "-updated" took roughly every second frame away from the reader.
    /// </summary>
    internal bool WaitsForUpdated { get; }

    /// <summary>Whether this side waits for the "-request" event (the writers do; a reader waiting for its own request would swallow it).</summary>
    internal bool WaitsForRequests { get; }

    protected SignaledMemoryObject(string fileName, bool waitForUpdated = true, bool waitForRequests = true)
    {
        WaitsForUpdated = waitForUpdated;
        WaitsForRequests = waitForRequests;
        var updatedHandleName = fileName + "-updated";
        var handleSecurity = GetEventWaitHandleSecurity();

        const EventWaitHandleRights rights = EventWaitHandleRights.TakeOwnership | EventWaitHandleRights.Synchronize | EventWaitHandleRights.FullControl;
        ObjectUpdatedHandle =
            EventWaitHandleAcl.TryOpenExisting(updatedHandleName, rights, out var updatedHandle) ?
                updatedHandle : EventWaitHandleAcl.Create(false, EventResetMode.AutoReset, updatedHandleName, out _, handleSecurity);
        ObjectUpdatedHandle.Reset();    //in case it wasn't cleared automatically

        var requestedHandleName = fileName + "-request";
        UpdateRequestedHandle =
            EventWaitHandleAcl.TryOpenExisting(requestedHandleName, rights, out var requestedHandle) ?
                requestedHandle : EventWaitHandleAcl.Create(false, EventResetMode.AutoReset, requestedHandleName, out _, handleSecurity);
        
        MemorySharedEventThread.AddObject(this);
    }

    private static EventWaitHandleSecurity GetEventWaitHandleSecurity()
    {
        var eventWaitHandleSecurity = new EventWaitHandleSecurity();
        
        var anonymous = new SecurityIdentifier(WellKnownSidType.AnonymousSid, null);
        eventWaitHandleSecurity.AddAccessRule(
            new EventWaitHandleAccessRule(anonymous, EventWaitHandleRights.TakeOwnership, AccessControlType.Allow)
        );
        eventWaitHandleSecurity.AddAccessRule(
            new EventWaitHandleAccessRule(anonymous, EventWaitHandleRights.Synchronize, AccessControlType.Allow)
        );
        eventWaitHandleSecurity.AddAccessRule(
            new EventWaitHandleAccessRule(anonymous, EventWaitHandleRights.FullControl, AccessControlType.Allow)
        );
        
        var everyone = new SecurityIdentifier(WellKnownSidType.WorldSid, null);
        eventWaitHandleSecurity.AddAccessRule(
            new EventWaitHandleAccessRule(everyone, EventWaitHandleRights.TakeOwnership, AccessControlType.Allow)
        );
        eventWaitHandleSecurity.AddAccessRule(
            new EventWaitHandleAccessRule(everyone, EventWaitHandleRights.FullControl, AccessControlType.Allow)
        );
        eventWaitHandleSecurity.AddAccessRule(
            new EventWaitHandleAccessRule(everyone, EventWaitHandleRights.FullControl, AccessControlType.Allow)
        );
        
        return eventWaitHandleSecurity;
    }

    public void RequestUpdate()
    {
        if (Disposed)
        {
            return;
        }
        ObjectUpdatedHandle.Reset();
        UpdateRequestedHandle.Set();
    }

    protected void SignalUpdated()
    {
        if (Disposed)
        {
            return;
        }
        ObjectUpdatedHandle.Set();
    }

    protected virtual void Dispose(bool disposing)
    {
        Disposed = true;
        if (!disposing) return;
        MemorySharedEventThread.RemoveObject(this);
            
        ObjectUpdatedHandle.Dispose();
        UpdateRequestedHandle.Dispose();
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    internal void OnUpdated()
    {
        Updated?.Invoke(this, EventArgs.Empty);
    }

    internal void OnUpdateRequested()
    {
        UpdateRequested?.Invoke(this, EventArgs.Empty);
    }

    protected void WaitForUpdate()
    {
        if (Disposed)
        {
            return;
        }
#if DEBUG
        ObjectUpdatedHandle.WaitOne();
#else
        ObjectUpdatedHandle.WaitOne(TimeSpan.FromSeconds(5));
#endif
    }
}