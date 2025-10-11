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
    public int GamePid { get; private set; }
    public string GameProcess { get; private set; } = string.Empty;

    private SdkHandler? _sdkHandler;

    public RunningProcessMonitor? RunningProcessMonitor { get; set; }

    public void SetSdkHandler(SdkHandler gameHandlerSdkHandler, int gamePid)
    {
        IsGameConnected = true;
        GamePid = gamePid;
        _sdkHandler = gameHandlerSdkHandler;
        gameHandlerSdkHandler.ColorsUpdated += OnColorsUpdated;
        
        SdkHandlerOnGameConnected();
    }

    public void ClearSdkHandler()
    {
        if (_sdkHandler == null)
        {
            return;
        }

        IsGameConnected = false;
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

    private void SdkHandlerOnGameConnected()
    {
        var process = Process.GetProcessById(GamePid);
        GameProcess = process.ProcessName + ".exe";
        IsGameConnected = true;
        GameChanged?.Invoke(this, EventArgs.Empty);
    }
}