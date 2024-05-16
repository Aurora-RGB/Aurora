using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using AuroraRgb.Devices;
using AuroraRgb.Modules.GameStateListen;
using Hardcodet.Wpf.TaskbarNotification;

namespace AuroraRgb;

public sealed class AuroraControlInterface(Task<IpcListener?> listener)
{
    public TaskbarIcon? TrayIcon { private get; set; }
    public DeviceManager? DeviceManager { private get; set; }

    public async Task Initialize()
    {
        var ipcListener = await listener;
        if (ipcListener != null)
        {
            ipcListener.AuroraCommandReceived += OnAuroraCommandReceived;
        }
    }

    public void ShowErrorNotification(string message)
    {
        TrayIcon?.ShowBalloonTip("Aurora", message, BalloonIcon.Error);
    }

    private void OnAuroraCommandReceived(object? sender, string e)
    {
        switch (e)
        {
            case "restartAurora":
                RestartAurora();
                break;
            case "restartAll":
                ShutdownDevices().Wait();
                RestartAurora();
                break;
            case "shutdown":
                ShutdownDevices().Wait();
                ExitApp();
                break;
            case "restartDevices":
                RestartDevices().Wait();
                break;
            case "quitDevices":
                ShutdownDevices().Wait();
                break;
            case "startDevices":
                if (DeviceManager == null)
                {
                    return;
                }
                DeviceManager.InitializeDevices().Wait();
                break;
        }
    }
    
    public void ExitApp()
    {
        //to only shutdown Aurora itself
        DeviceManager?.Detach();
        if (Thread.CurrentThread == Application.Current.Dispatcher.Thread)
        {
            Application.Current.Shutdown();
        }
        else
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                Application.Current.Shutdown();
            });
        }
    }

    public async Task RestartDevices()
    {
        if (DeviceManager == null)
        {
            return;
        }
        await DeviceManager.ResetDevices();
    }

    public async Task ShutdownDevices()
    {
        if (DeviceManager == null)
        {
            return;
        }
        await DeviceManager.ShutdownDevices();
    }

    public void RestartAurora()
    {
        //so that we don't restart device manager
        DeviceManager?.Detach();

        Application.Current.Dispatcher.Invoke(RestartAuroraApplicationCall);
    }

    private void RestartAuroraApplicationCall()
    {
        var auroraPath = Path.Combine(Global.ExecutingDirectory, Global.AuroraExe);

        var currentProcess = Environment.ProcessId;
        var minimizedArg = Application.Current?.MainWindow?.Visibility == Visibility.Visible ? "" : " -minimized";
        Process.Start(new ProcessStartInfo
        {
            FileName = auroraPath,
            Arguments = $"-restart {currentProcess}{minimizedArg}"
        });

        ExitApp();
    }
}