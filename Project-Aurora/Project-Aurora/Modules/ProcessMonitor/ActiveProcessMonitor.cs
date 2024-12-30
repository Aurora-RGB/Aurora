using System;
using System.Diagnostics;
using AuroraRgb.Settings;
using AuroraRgb.Utils;

namespace AuroraRgb.Modules.ProcessMonitor;

public sealed class ActiveProcessMonitor : IDisposable
{
	private const uint WinEventOutOfContext = 0;
	private const uint EventSystemForeground = 3;

	private string _processPath = string.Empty;
	public string ProcessName {
		get => _processPath;
		private set
		{
			_processPath = value;
			ActiveProcessChanged?.Invoke(this, EventArgs.Empty);
		}
	}
	public string ProcessTitle { get; private set; } = string.Empty;

	public int ProcessId { get; private set; }
	
	public event EventHandler? ActiveProcessChanged;

	private readonly User32.WinEventDelegate _dele;
	private readonly IntPtr _setWinEventHook;

	internal ActiveProcessMonitor()
	{
		_dele = WinEventProc;
		if (Global.Configuration.DetectionMode is ApplicationDetectionMode.WindowsEvents or ApplicationDetectionMode.EventsAndForeground)
		{
			_setWinEventHook = User32.SetWinEventHook(EventSystemForeground, EventSystemForeground, 
				IntPtr.Zero, _dele, 0, 0, WinEventOutOfContext);
		}
	}

	private void WinEventProc(IntPtr hWinEventHook, uint eventType, IntPtr windowHandle, int idObject, int idChild, uint dwEventThread, uint dwmsEventTime)
	{
		SetActiveWindowProperties(windowHandle);
	}

	public void UpdateActiveProcessPolling()
	{
		var windowHandle = User32.GetForegroundWindow();
		SetActiveWindowProperties(windowHandle);
	}

	private void SetActiveWindowProperties(IntPtr windowHandle)
	{
		try
		{
			if (User32.GetWindowThreadProcessId(windowHandle, out var pid) > 0)
			{
				using var process = Process.GetProcessById((int)pid);
				ProcessTitle = process.MainWindowTitle;
				ProcessName = process.ProcessName + ".exe";
				ProcessId = (int)pid;
				Global.logger.Debug("New foreground process: {Process} ({Pid})", ProcessName, ProcessId);
				return;
			}
		}
		catch (Exception exc)
		{
			Global.logger.Error(exc, "Exception in GetActiveWindowsProcessName");
		}

		ProcessTitle = string.Empty;
		ProcessName = string.Empty;
		ProcessId = 0;
	}

	public void Dispose()
	{
		if (_setWinEventHook != IntPtr.Zero)
		{
			User32.UnhookWinEvent(_setWinEventHook);
		}
	}
}