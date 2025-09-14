using System;
using System.Collections.Generic;
using System.Diagnostics;
using AuroraRgb.Modules.ProcessMonitor;
using iCUE_ReverseEngineer;
using iCUE_ReverseEngineer.Icue.Sdk;

namespace AuroraRgb.Modules.Icue;

public class IcueSdk
{
    public event EventHandler<EventArgs>? GameChanged;
    public event EventHandler<EventArgs>? ColorsUpdated;
    public Dictionary<IcueLedId, IcueColor> Colors { get; private set; } = [];
    public bool IsGameConnected { get; private set; }
    public long GamePid { get; private set; }
    public string GameProcess { get; private set; } = string.Empty;

    private SdkHandler? _sdkHandler;

    public RunningProcessMonitor? RunningProcessMonitor { get; set; }

    public void SetSdkHandler(SdkHandler gameHandlerSdkHandler, int gamePid)
    {
        var process = Process.GetProcessById(gamePid);
        GameProcess = process.ProcessName + ".exe";

        IsGameConnected = true;
        GamePid = gamePid;
        _sdkHandler = gameHandlerSdkHandler;
        _sdkHandler.GameConnected += SdkHandlerOnGameConnected;
        _sdkHandler.ColorsUpdated += OnColorsUpdated;
    }

    public void ClearSdkHandler()
    {
        if (_sdkHandler == null)
        {
            return;
        }

        IsGameConnected = false;
        _sdkHandler.GameConnected -= SdkHandlerOnGameConnected;
        _sdkHandler.ColorsUpdated -= OnColorsUpdated;
        _sdkHandler = null;

        GamePid = 0;
        GameProcess = string.Empty;
        GameChanged?.Invoke(this, EventArgs.Empty);
    }

    private void OnColorsUpdated(object? sender, EventArgs e)
    {
        if (_sdkHandler == null)
        {
            return;
        }

        Colors = _sdkHandler.LedColors;
        ColorsUpdated?.Invoke(this, e);
    }

    private void SdkHandlerOnGameConnected(object? sender, EventArgs e)
    {
        IsGameConnected = true;
        GameChanged?.Invoke(this, e);
    }
}