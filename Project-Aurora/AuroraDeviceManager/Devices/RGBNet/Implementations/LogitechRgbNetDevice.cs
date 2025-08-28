using AuroraDeviceManager.Utils;
using Microsoft.Win32;
using RGB.NET.Core;
using RGB.NET.Devices.Logitech;

namespace AuroraDeviceManager.Devices.RGBNet.Implementations;

public sealed class LogitechRgbNetDevice() : RgbNetDevice(true)
{
    private bool _sdkDetectedOff;

    protected override IRGBDeviceProvider Provider => LogitechDeviceProvider.Instance;

    public override string DeviceName => "Logitech (RGB.NET)";

    protected override async Task ConfigureProvider(CancellationToken cancellationToken)
    {
        await base.ConfigureProvider(cancellationToken);
        
        // normalise dll path
        var relativeDllPath = "x64/LogitechLedEnginesWrapper.dll";
        var absoluteDllPath = Path.GetFullPath(relativeDllPath);
        LogitechDeviceProvider.PossibleX64NativePaths.Clear();
        LogitechDeviceProvider.PossibleX64NativePaths.Add(absoluteDllPath);

        var isSdkRunning = ProcessUtils.IsProcessRunning("lghub_agent") || ProcessUtils.IsProcessRunning("lcore");
        if (!isSdkRunning)
        {
            _sdkDetectedOff = true;
            throw new DeviceProviderException(new ApplicationException("LGS or GHUB need to be running!"), false);
        }

        if (_sdkDetectedOff)
        {
            await Task.Delay(5000, cancellationToken);
        }

        _sdkDetectedOff = false;
    }

    protected override void OnInitialized()
    {
        SystemEvents.SessionSwitch += SystemEventsOnSessionSwitch;
    }

    protected override bool OnShutdown()
    {
        SystemEvents.SessionSwitch -= SystemEventsOnSessionSwitch;

        return true;
    }

    #region Event handlers

    private async void SystemEventsOnSessionSwitch(object? sender, SessionSwitchEventArgs e)
    {
        if (!IsInitialized)
            return;
        
        switch (e.Reason)
        {
            case SessionSwitchReason.SessionLock:
            case SessionSwitchReason.SessionLogoff:
                await Shutdown();
                SystemEvents.SessionSwitch += SystemEventsOnSessionSwitch;
                break;
            case SessionSwitchReason.SessionLogon:
            case SessionSwitchReason.SessionUnlock:
                await Task.Delay(TimeSpan.FromSeconds(4));
                await Initialize();
                break;
        }
    }

    #endregion
}