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

    private readonly byte[] _logitechBitmap = new byte[LogitechGSDK.LOGI_LED_BITMAP_SIZE];
    private SimpleColor[] _speakers = new SimpleColor[4];
    private SimpleColor _mousepad;
    private readonly SimpleColor[] _mouse = new SimpleColor[3];
    private readonly SimpleColor[] _headset = new SimpleColor[4];
    private DeviceKeys _genericKey;

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

        if (Global.DeviceConfig.VarRegistry.GetVariable<bool>($"{DeviceName}_override_dll"))
            LogitechGSDK.GHUB = Global.DeviceConfig.VarRegistry.GetVariable<LGDLL>($"{DeviceName}_override_dll_option") == LGDLL.GHUB;
        else
            LogitechGSDK.GHUB = ghubRunning;

        LogInfo($"Trying to initialize Logitech using the dll for {(LogitechGSDK.GHUB ? "GHUB" : "LGS")}");

        if (LogitechGSDK.LogiLedInit() && LogitechGSDK.LogiLedSaveCurrentLighting())
        {
            //logitech says to wait a bit of time between Init() and SetLighting()
            //This didnt seem to be needed in the past but I feel like 100ms might 
            //fix some weird issues without any noticeable disadvantages
            await Task.Delay(100).ConfigureAwait(false);
            if (Global.DeviceConfig.VarRegistry.GetVariable<bool>($"{DeviceName}_set_default"))
                LogitechGSDK.LogiLedSetLighting(Global.DeviceConfig.VarRegistry.GetVariable<SimpleColor>($"{DeviceName}_default_color"));
            IsInitialized = true;
            return true;
        }

        SystemEvents.SessionSwitch += SystemEvents_SessionSwitch;

        IsInitialized = false;
        return false;
    }

    protected override Task Shutdown()
    {
        LogitechGSDK.LogiLedRestoreLighting();
        LogitechGSDK.LogiLedShutdown();
        SystemEvents.SessionSwitch -= SystemEvents_SessionSwitch;
        IsInitialized = false;
        return Task.CompletedTask;
    }

    // Handle Logon Event
    async void SystemEvents_SessionSwitch(object? sender, SessionSwitchEventArgs e)
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

    private static void SetAll(IList<SimpleColor> colors, SimpleColor color)
    {
        for (var i = 0; i < colors.Count; i++)
        {
            colors[i] = color;
        }
    }

    protected override Task<bool> UpdateDevice(Dictionary<DeviceKeys, SimpleColor> keyColors, DoWorkEventArgs e, bool forced = false)
    {
        if (!IsInitialized)
            return Task.FromResult(false);

        //reset keys to peripheral_logo here so if we dont find any better color for them,
        //at least the leds wont turn off :)
        if (keyColors.TryGetValue(_genericKey, out var peripheralColor))
        {
            SetAll(_speakers, peripheralColor);
            _mousepad = peripheralColor;
            SetAll(_mouse, peripheralColor);
            SetAll(_headset, peripheralColor);
        }

        foreach (var (key, color) in keyColors)
        {
            UpdateLed(color, key);
        }

        if (!Global.DeviceConfig.DevicesDisableMouse)
        {
            for (var i = 0; i < _mouse.Length; i++)
            {
                LogitechGSDK.LogiLedSetLightingForTargetZone(DeviceType.Mouse, i, _mouse[i]);
            }

            LogitechGSDK.LogiLedSetLightingForTargetZone(DeviceType.Mousemat, 0, _mousepad);
        }
        if (!Global.DeviceConfig.DevicesDisableHeadset)
        {
            for (var i = 0; i < _headset.Length; i++)
            {
                LogitechGSDK.LogiLedSetLightingForTargetZone(DeviceType.Headset, i, _headset[i]);
            }

            for (var i = 0; i < _speakers.Length; i++)//speakers have 4 leds
            {
                LogitechGSDK.LogiLedSetLightingForTargetZone(DeviceType.Speaker, i, _speakers[i]);
            }
        }
        if (!Global.DeviceConfig.DevicesDisableKeyboard)
        {
            IsInitialized &= LogitechGSDK.LogiLedSetLightingFromBitmap(_logitechBitmap);
        }

        return Task.FromResult(IsInitialized);
    }

    private void UpdateLed(SimpleColor color, DeviceKeys key)
    {
        if (color is { A: 0 })
        {
            return;
        }
        if (key == DeviceKeys.Peripheral)
        {
            LogitechGSDK.LogiLedSetLighting(color);
            return;
        }

        #region keyboard
        if (LedMaps.BitmapMap.TryGetValue(key, out var index))
        {
            _logitechBitmap[index] = color.B;
            _logitechBitmap[index + 1] = color.G;
            _logitechBitmap[index + 2] = color.R;
            _logitechBitmap[index + 3] = color.A;
        }

        if (!Global.DeviceConfig.DevicesDisableKeyboard && LedMaps.KeyMap.TryGetValue(key, out var logiKey))
            IsInitialized &= LogitechGSDK.LogiLedSetLightingForKeyWithKeyName(logiKey, color);
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
        variableRegistry.Register($"{DeviceName}_override_dll", false, "Override DLL", null, null, "Requires restart to take effect");
        variableRegistry.Register($"{DeviceName}_override_dll_option", LGDLL.GHUB, "Override DLL Selection", null, null, "Requires restart to take effect");
        variableRegistry.Register($"{DeviceName}_devicekey", DeviceKeys.Peripheral_Logo, "Key to Use", DeviceKeys.MOUSEPADLIGHT15, DeviceKeys.Peripheral);
    }
}