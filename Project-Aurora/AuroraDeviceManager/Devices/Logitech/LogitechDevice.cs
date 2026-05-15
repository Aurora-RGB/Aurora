using System.ComponentModel;
using AuroraDeviceManager.Utils;
using Common;
using Common.Devices;
using Common.Devices.Logitech;
using Microsoft.Win32;

namespace AuroraDeviceManager.Devices.Logitech;

public class LogitechDevice : DefaultDevice
{
    public override string DeviceName => "Logitech";

    private readonly byte[] _logitechBitmap = new byte[LogitechSdk.LOGI_LED_BITMAP_SIZE];
    private readonly SimpleColor[] _speakers = new SimpleColor[4];
    private SimpleColor _mousepad;
    private readonly SimpleColor[] _mouse = new SimpleColor[3];
    private readonly SimpleColor[] _headset = new SimpleColor[4];
    private DeviceKeys _genericKey;

    private readonly LogitechSdk _logitechSdk = new();

    protected override async Task<bool> DoInitialize(CancellationToken cancellationToken)
    {
        _genericKey = Global.DeviceConfig.VarRegistry.GetVariable<DeviceKeys>($"{DeviceName}_devicekey");
        var ghubRunning = ProcessUtils.IsProcessRunning("lghub_agent");
        var lgsRunning = ProcessUtils.IsProcessRunning("lcore");

        if (!ghubRunning && !lgsRunning)
        {
            IsInitialized = false;
            return false;
        }

        LogInfo("Trying to initialize Logitech using the dll");

        if (_logitechSdk.LogiLedInitWithName("AuroraRgb"))
        {
            //logitech says to wait a bit of time between Init() and SetLighting()
            //This didn't seem to be needed in the past, but I feel like 100ms might 
            //fix some weird issues without any noticeable disadvantages
            await Task.Delay(100, cancellationToken);
            _logitechSdk.LogiLedSetTargetDevice(LogiLedType.All);
            _logitechSdk.LogiLedExcludeKeysFromBitmap([]);
            if (Global.DeviceConfig.VarRegistry.GetVariable<bool>($"{DeviceName}_set_default"))
                _logitechSdk.LogiLedSetLighting(Global.DeviceConfig.VarRegistry.GetVariable<SimpleColor>($"{DeviceName}_default_color"));
            IsInitialized = true;
            return true;
        }

        SystemEvents.SessionSwitch += SystemEvents_SessionSwitch;

        IsInitialized = false;
        return false;
    }

    protected override Task Shutdown()
    {
        _logitechSdk.LogiLedShutdown();
        SystemEvents.SessionSwitch -= SystemEvents_SessionSwitch;
        IsInitialized = false;
        return Task.CompletedTask;
    }

    // Handle Logon Event
    private async void SystemEvents_SessionSwitch(object? sender, SessionSwitchEventArgs e)
    {
        switch (e.Reason)
        {
            case SessionSwitchReason.SessionLock:
            case SessionSwitchReason.SessionLogoff:
                try
                {
                    await Shutdown();
                }catch{ /* ignore */}
                SystemEvents.SessionSwitch -= SystemEvents_SessionSwitch;
                SystemEvents.SessionSwitch += SystemEvents_SessionSwitch;
                break;
            case SessionSwitchReason.SessionLogon:
            case SessionSwitchReason.SessionUnlock:
                await Task.Delay(TimeSpan.FromSeconds(4));
                await Initialize();
                break;
        }
    }

    protected override Task<bool> UpdateDevice(Dictionary<DeviceKeys, SimpleColor> keyColors, DoWorkEventArgs e, bool forced = false)
    {
        if (!IsInitialized)
            return Task.FromResult(false);

        //reset keys to peripheral_logo here so if we don't find any better color for them,
        //at least the leds won't turn off :)
        if (keyColors.TryGetValue(_genericKey, out var peripheralColor))
        {
            _logitechSdk.LogiLedSetLighting(peripheralColor);
        }

        foreach (var (key, color) in keyColors)
        {
            UpdateLed(color, key);
        }

        if (!Global.DeviceConfig.DevicesDisableMouse)
        {
            for (var i = 0; i < _mouse.Length; i++)
            {
                _logitechSdk.LogiLedSetLightingForTargetZone(DeviceType.Mouse, i, _mouse[i]);
            }

            _logitechSdk.LogiLedSetLightingForTargetZone(DeviceType.Mousemat, 0, _mousepad);
        }
        if (!Global.DeviceConfig.DevicesDisableHeadset)
        {
            for (var i = 0; i < _headset.Length; i++)
            {
                _logitechSdk.LogiLedSetLightingForTargetZone(DeviceType.Headset, i, _headset[i]);
            }

            for (var i = 0; i < _speakers.Length; i++)//speakers have 4 leds
            {
                _logitechSdk.LogiLedSetLightingForTargetZone(DeviceType.Speaker, i, _speakers[i]);
            }
        }
        if (!Global.DeviceConfig.DevicesDisableKeyboard)
        {
            _logitechSdk.LogiLedSetLightingFromBitmap(_logitechBitmap);

            foreach (var (key, nonBitmapKey) in LedMaps.KeyMap)
            {
                if (!keyColors.TryGetValue(key, out var color)) continue;
                {
                    _logitechSdk.LogiLedSetLightingForKeyWithKeyName(nonBitmapKey, color);
                }
            }
        }

        return Task.FromResult(IsInitialized);
    }

    private void UpdateLed(SimpleColor color, DeviceKeys key)
    {
        #region keyboard

        if (LedMaps.BitmapMap.TryGetValue(key, out var index))
        {
            _logitechBitmap[index] = color.B;
            _logitechBitmap[index + 1] = color.G;
            _logitechBitmap[index + 2] = color.R;
            _logitechBitmap[index + 3] = color.A;
        }

        #endregion

        #region peripherals

        if (!LedMaps.PeripheralMap.TryGetValue(key, out var peripheral)) return;
        switch (peripheral.type)
        {
            case DeviceType.Mouse:
                _mouse[peripheral.zone] = color;
                break;
            case DeviceType.Mousemat:
                _mousepad = color;
                break;
            case DeviceType.Headset:
                _headset[peripheral.zone] = color;
                break;
            case DeviceType.Speaker:
                _speakers[peripheral.zone] = color;
                break;
        }
        #endregion
    }

    protected override void RegisterVariables(VariableRegistry variableRegistry)
    {
        variableRegistry.Register($"{DeviceName}_set_default", false, "Set Default Color");
        variableRegistry.Register($"{DeviceName}_default_color", SimpleColor.FromRgba(255, 255, 255), "Default Color");
        variableRegistry.Register($"{DeviceName}_devicekey", DeviceKeys.Peripheral_Logo, "Key to Use", DeviceKeys.MOUSEPADLIGHT15, DeviceKeys.Peripheral);
    }
}