﻿using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using Bloody.NET;
using Common;
using Common.Devices;
using Common.Utils;

namespace AuroraDeviceManager.Devices.Bloody;

public class BloodyDevice : DefaultDevice
{
    public override string DeviceName => "Bloody";
    protected override string DeviceInfo => IsInitialized ? GetDeviceNames() : base.DeviceInfo;
    private readonly Stopwatch _updateDelayStopWatch = new();

    private BloodyKeyboard? _keyboard;

    private event EventHandler<Dictionary<DeviceKeys, SimpleColor>> DeviceUpdated;

    protected override Task<bool> DoInitialize()
    {
        _keyboard = BloodyKeyboard.Initialize();
        if(_keyboard != null)
        {
            DeviceUpdated += UpdateKeyboard;
        }

        _updateDelayStopWatch.Start();

        IsInitialized = _keyboard != null;
        return Task.FromResult(IsInitialized);
    }

    protected override Task Shutdown()
    {
        _keyboard?.Disconnect();
        DeviceUpdated -= UpdateKeyboard;

        _updateDelayStopWatch.Stop();

        IsInitialized = false;
        return Task.CompletedTask;
    }

    protected override Task<bool> UpdateDevice(Dictionary<DeviceKeys, SimpleColor> keyColors, DoWorkEventArgs e, bool forced = false)
    {
        var sendDelay = Global.DeviceConfig.VarRegistry.GetVariable<int>($"{DeviceName}_send_delay");
        if (_updateDelayStopWatch.ElapsedMilliseconds <= sendDelay)
            return Task.FromResult(false);

        DeviceUpdated?.Invoke(this, keyColors);
        _updateDelayStopWatch.Restart();
        return Task.FromResult(true);
    }
    protected override void RegisterVariables(VariableRegistry variableRegistry)
    {
        variableRegistry.Register($"{DeviceName}_send_delay", 32, "Send delay (ms)");
    }

    private void UpdateKeyboard(object? sender, Dictionary<DeviceKeys, SimpleColor> keyColors)
    {
        foreach (var key in keyColors)
        {
            if (BloodyKeyMap.KeyMap.TryGetValue(key.Key, out var bloodyKey))
                _keyboard.SetKeyColor(bloodyKey, (Color)CommonColorUtils.CorrectWithAlpha(key.Value));
        }

        _keyboard.Update();
    }

    private string GetDeviceNames()
    {
        return _keyboard != null ? " Keyboard" : "";
    }
}