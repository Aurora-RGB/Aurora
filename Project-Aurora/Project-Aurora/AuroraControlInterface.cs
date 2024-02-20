﻿using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using Aurora.Devices;
using Aurora.Modules.GameStateListen;
using Hardcodet.Wpf.TaskbarNotification;

namespace Aurora;

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
        Global.logger.Debug("Received command: {Command}", e);
        switch (e)
        {
            case "shutdown":
                ShutdownDevices().Wait();
                ExitApp();
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
        Application.Current.Shutdown();
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

        var auroraPath = Path.Combine(Global.ExecutingDirectory, "Aurora.exe");

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