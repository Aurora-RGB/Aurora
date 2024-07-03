using System;
using System.Diagnostics;
using System.Reflection;
using System.Windows.Automation;
using Microsoft.Collections.Extensions;

namespace AuroraRgb.Modules.ProcessMonitor;

public sealed class WindowProcess(int windowHandle) : IEquatable<WindowProcess>
{
    public int ProcessId { get; internal set; }
    public string ProcessName { get; internal set; } = string.Empty;
    public int WindowHandle { get; } = windowHandle;

    public bool Equals(WindowProcess? other)
    {
        if (ReferenceEquals(null, other)) return false;
        if (ReferenceEquals(this, other)) return true;
        return WindowHandle == other.WindowHandle;
    }

    public override bool Equals(object? obj)
    {
        if (ReferenceEquals(null, obj)) return false;
        if (ReferenceEquals(this, obj)) return true;
        return obj.GetType() == GetType() && Equals((WindowProcess)obj);
    }

    public override int GetHashCode()
    {
        return WindowHandle;
    }
}

public class WindowEventArgs(string processName, int processId, int windowHandle, bool opened): EventArgs
{
    public string ProcessName => processName;
    public int ProcessId => processId;
    public int WindowHandle => windowHandle;
    public bool Opened => opened;
}

public sealed class WindowListener : IDisposable
{
    public static WindowListener Instance { get; set; }

    // event that sends process id of new window
    public event EventHandler<WindowEventArgs>? WindowCreated;
    public event EventHandler<WindowEventArgs>? WindowDestroyed;

    public readonly MultiValueDictionary<string, WindowProcess> ProcessWindowsMap = new();
    private static readonly string Aurora = Assembly.GetExecutingAssembly().GetName().Name ?? "Aurora";

    public void StartListening()
    {
        Automation.AddAutomationEventHandler(WindowPattern.WindowOpenedEvent, AutomationElement.RootElement, TreeScope.Descendants, WindowDetected);
    }

    private void WindowDetected(object? sender, AutomationEventArgs e)
    {
        try
        {
            var element = (AutomationElement)sender;
            using var process = Process.GetProcessById(element.Current.ProcessId);
            if (process.ProcessName == Aurora)
            {
                return;
            }

            var name = process.ProcessName + ".exe";
            var windowHandle = element.Current.NativeWindowHandle;

            var processId = process.Id;
            Automation.AddAutomationEventHandler(WindowPattern.WindowClosedEvent, element, TreeScope.Element, (_, _) =>
            {
                ProcessWindowsMap.Remove(name, new WindowProcess(windowHandle));
                WindowDestroyed?.Invoke(this, new WindowEventArgs(name, processId, windowHandle, false));
            });

            //To make sure window close event can be fired, we fire open event after subscribing to close event
            ProcessWindowsMap.Add(name, new WindowProcess(windowHandle) { ProcessId = element.Current.ProcessId, ProcessName = name, });
            WindowCreated?.Invoke(this, new WindowEventArgs(name, processId, windowHandle, true));
        }
        catch
        {
            //ignored
        }
    }

    public void Dispose()
    {
        Automation.RemoveAutomationEventHandler(WindowPattern.WindowOpenedEvent, AutomationElement.RootElement, WindowDetected);
    }
}