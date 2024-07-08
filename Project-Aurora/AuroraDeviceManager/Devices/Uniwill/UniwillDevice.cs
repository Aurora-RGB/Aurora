using System.ComponentModel;
using Common;
using Common.Utils;
using Microsoft.Win32;
using UniwillSDKDLL;
using Timer = System.Timers.Timer;

namespace AuroraDeviceManager.Devices.Uniwill;

enum GAMECENTERTYPE
{
    NONE = 0,
    GAMINGTCENTER = 1,
    CONTROLCENTER = 2
}

public class UniwillDevice : DefaultDevice
{
    private string _deviceName = "Uniwill";

    private Timer? _regTimer;
    private const string Root = "HKEY_LOCAL_MACHINE";
    private const string Subkey = @"SOFTWARE\OEM\Aurora";
    private const string KeyName = Root + "\\" + Subkey;
    private int _switchOn;

    private AuroraInterface? _keyboard;

    private GAMECENTERTYPE _gamingCenterType = 0;

    private const float Brightness = 1f;
    private bool _bRefreshOnce = true; // This is used to refresh effect between Row-Type and Fw-Type change or layout light level change

    public UniwillDevice()
    {
        ChoiceGamingCenter();
    }

    private void ChoiceGamingCenter()
    {
        _gamingCenterType = CheckGc();

        if (_gamingCenterType == GAMECENTERTYPE.GAMINGTCENTER)
        {
            _regTimer = new Timer();
            _regTimer.Interval = 300;
            _regTimer.Elapsed += OnRegChanged;
            _regTimer.Stop();
            _regTimer.Start();

        }
    }

    private GAMECENTERTYPE CheckGc()
    {
        var control = (int?)Registry.GetValue(KeyName, "AuroraSwitch", null);
            
        if(control.HasValue)
        {
            _gamingCenterType = GAMECENTERTYPE.GAMINGTCENTER;
            _switchOn = control.Value;
        }
        else
        {
            _gamingCenterType = GAMECENTERTYPE.NONE;
            _switchOn = 0;
        }
        return _gamingCenterType;
    }

    private bool CheckGcPower()
    {
        if (_gamingCenterType != GAMECENTERTYPE.GAMINGTCENTER)
        {
            return true;
        }

        var control = (int)Registry.GetValue(KeyName, "AuroraSwitch", 0);
        return control != 0;
    }

    private void OnRegChanged(object? sender, EventArgs e)
    {
        var newSwtich = (int)Registry.GetValue(KeyName, "AuroraSwitch", 0);
        if (_switchOn == newSwtich) return;

        _switchOn = newSwtich;
        if (CheckGcPower())
        {
            Initialize().Wait();
        }
        else
        {
            _bRefreshOnce = true;
            IsInitialized = false;
            Shutdown().Wait();
        }
    }

    public override string DeviceName => _deviceName;

    protected override Task<bool> DoInitialize(CancellationToken cancellationToken)
    {
        if (IsInitialized || !CheckGcPower()) return Task.FromResult(IsInitialized);
        try
        {
            _deviceName = KeyboardFactory.GetOEMName();

            _keyboard = KeyboardFactory.CreateHIDDevice("hidkeyboard");
            if (_keyboard != null)
            {
                _bRefreshOnce = true;
                IsInitialized = true;
                //SetBrightness();
                return Task.FromResult(true);
            }

            IsInitialized = false;
            return Task.FromResult(false);
        }
        catch
        {
            Global.Logger.Error("Uniwill device error!");
        }
        // Mark Initialized = FALSE
        IsInitialized = false;
        return Task.FromResult(false);

    }

    protected override Task Shutdown()
    {
        if (!IsInitialized) return Task.CompletedTask;
        if (CheckGcPower())
        {
            _keyboard?.release();
        }

        _bRefreshOnce = true;
        IsInitialized = false;

        return Task.CompletedTask;
    }

    protected override Task<bool> UpdateDevice(Dictionary<Common.Devices.DeviceKeys, SimpleColor> keyColors, DoWorkEventArgs e, bool forced = false)
    {
        if (e.Cancel) return Task.FromResult(false);

        //Alpha necessary for Global Brightness modifier
        var adjustedColors = keyColors.Select(AdjustBrightness);

        var ret = _keyboard?.SetEffect(0x32, 0x00, _bRefreshOnce, adjustedColors, e) ?? false;

        _bRefreshOnce = false;

        return Task.FromResult(ret);
    }

    private static KeyValuePair<Common.Devices.DeviceKeys, SimpleColor> AdjustBrightness(KeyValuePair<Common.Devices.DeviceKeys, SimpleColor> kc)
    {
        var color = CommonColorUtils.MultiplyColorByScalar(kc.Value, kc.Value.A / 255.0D * Brightness) with{ A = 255};
        var newEntry = new KeyValuePair<Common.Devices.DeviceKeys, SimpleColor>(kc.Key, color);
        kc = newEntry;
        return kc;
    }
}