using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Management;
using System.Threading;
using System.Threading.Tasks;

namespace AuroraRgb.Modules.ProcessMonitor;

public class ProcessStarted(string processName) : EventArgs
{
    public string ProcessName { get; } = processName;
}

public class ProcessStopped(string processName) : EventArgs
{
    public string ProcessName { get; } = processName;
}

/// <summary>
/// Class that monitors running processes using the <see cref="ManagementEventWatcher"/> to update when new processes are started
/// or existing ones are terminated. This means that it is not relying on <see cref="Process.GetProcesses()"/> which is a much
/// more intensive task and causes lag when run often. The only minor downside is that this will not instantly detected when a
/// process closes and can delay by about 2 seconds, though this really shouldn't be an issue since this isn't required for the
/// profile switching - only the overlay toggling.
/// </summary>
public sealed class RunningProcessMonitor : IDisposable {
    public event EventHandler<ProcessStarted>? ProcessStarted;
    public event EventHandler<ProcessStopped>? ProcessStopped;

    private readonly ReaderWriterLockSlim _lock = new(LockRecursionPolicy.SupportsRecursion);

    /// <summary>A list of all currently running processes (and how many instances are running).</summary>
    /// <remarks>The reason for the count is so that if two processes of the same file are running and one is closed, we can know
    /// that the other is still running.</remarks>
    private readonly Dictionary<string, int> _runningProcesses;

    private readonly ManagementEventWatcher _startWatcher;
    private readonly ManagementEventWatcher _shortStopWatcher;
    private readonly ManagementEventWatcher _longStopWatcher;

    /// <summary>
    /// Creates a new instance of the <see cref="RunningProcessMonitor"/>, which performs an initial scan of running
    /// processes and then sets up the watchers with their relevant commands.
    /// </summary>
    internal RunningProcessMonitor() {
        // Fetch all processes running now
        _runningProcesses = Process.GetProcesses()
            .Select(p =>
            {
                try
                {
                    return p.ProcessName.ToLower() + ".exe";
                }
                finally
                {
                    p.Close();
                }
            })
            .GroupBy(name => name)
            .ToDictionary(g => g.First(), g => g.Count());

        // Listen for new processes
        _startWatcher = new ManagementEventWatcher("SELECT ProcessName FROM Win32_ProcessStartTrace");
        _startWatcher.EventArrived += OnProcessStarted;

        // Listen for closed processes 
        _shortStopWatcher = new ManagementEventWatcher("SELECT ProcessName FROM Win32_ProcessStopTrace");
        _shortStopWatcher.EventArrived += ShortProcessStopped;

        // Listen for closed processes 
        var query = new WqlEventQuery("__InstanceDeletionEvent", TimeSpan.FromSeconds(1), "TargetInstance isa 'Win32_Process'");
        _longStopWatcher = new ManagementEventWatcher(query);
        _longStopWatcher.EventArrived += LongProcessStopped;

        try
        {
            _startWatcher.Start();
            _shortStopWatcher.Start();
            _longStopWatcher.Start();
        }
        catch (ManagementException e)
        {
            Global.logger.Fatal(e, "WMI is corrupt");
            Process.Start("explorer", "https://www.project-aurora.com/Docs/diagnostics/repair-wmi/");
        }
    }

    private void OnProcessStarted(object? sender, EventArrivedEventArgs e)
    {
        // Get the name of the started process
        if (e.NewEvent.Properties["ProcessName"].Value is not string name)
        {
            return;
        }

        name = name.ToLower();

        Task.Run(() =>
        {
            _lock.EnterWriteLock();
            // Set the dictionary to be the existing value + 1 or simply 1 if it doesn't exist already.
            _runningProcesses[name] = _runningProcesses.TryGetValue(name, out var i) ? i + 1 : 1;
            _lock.ExitWriteLock();
        
            ProcessStarted?.Invoke(this, new ProcessStarted(name));
        });
    }

    private void ShortProcessStopped(object? sender, EventArrivedEventArgs e)
    {
        // Get the name of the terminated process
        if (e.NewEvent.GetPropertyValue("ProcessName") is not string name)
        {
            return;
        }

        // Win32_ProcessStopTrace only gets us the first 10 characters of the process name, so we won't use it
        if (name.Length > 10 || !name.EndsWith(".exe"))
        {
            return;
        }
        
        name = name.ToLower();

        OnProcessStopped(name);
    }

    private void LongProcessStopped(object? sender, EventArrivedEventArgs e)
    {
        // Get the name of the terminated process
        if (e.NewEvent.GetPropertyValue("TargetInstance") is not ManagementBaseObject targetInstance)
        {
            return;
        }

        if (targetInstance.GetPropertyValue("Name") is not string name)
        {
            return;
        }

        // Win32_ProcessStopTrace only gets us the first 10 characters of the process name, we handle it here
        if (name.Length <= 10)
        {
            return;
        }
        
        name = name.ToLower();

        OnProcessStopped(name);
    }

    private void OnProcessStopped(string name)
    {
        Task.Run(() =>
        {
            _lock.EnterWriteLock();
            
            // Ensure the process exists in our dictionary
            if (_runningProcesses.TryGetValue(name, out var count))
            {
                if (count == 1) // If there is only 1 process currently running, remove it (since that must've been the one that terminated)
                    _runningProcesses.Remove(name);
                else // Else, simply decrement the process count number
                    _runningProcesses[name]--;
            }
            else
            {
                Global.logger.Warning("Closed process {ProcessName} didn't exist in the list", name);
            }

            _lock.ExitWriteLock();

            ProcessStopped?.Invoke(this, new ProcessStopped(name));
        });
    }

    /// <summary>
    /// Returns whether the given process name is detected as running or not.
    /// </summary>
    public bool IsProcessRunning(string name)
    {
        _lock.EnterReadLock();
        var isProcessRunning = _runningProcesses.ContainsKey(name);
        _lock.ExitReadLock();
        return isProcessRunning;
    }

    public void Dispose()
    {
        _startWatcher.EventArrived -= OnProcessStarted;
        _startWatcher.Stop();
        _startWatcher.Dispose();
        _shortStopWatcher.EventArrived -= ShortProcessStopped;
        _shortStopWatcher.Stop();
        _shortStopWatcher.Dispose();
    }
}