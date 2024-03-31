using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using AuroraRgb.Modules.GameStateListen;
using Common;
using Common.Devices;
using Common.Devices.RGBNet;
using RGB.NET.Core;

namespace AuroraRgb.Devices.RGBNet.Config;

public partial class DeviceMapping
{
    private readonly Task<DeviceManager> _deviceManager;
    private readonly Task<IpcListener?> _ipcListener;

    private readonly List<RemappableDevice> _devices = [];
    private readonly List<RgbNetKeyToDeviceKeyControl> _keys = [];

    private readonly Lazy<DeviceMappingConfig> _config = new(() => DeviceMappingConfig.Config);

    public DeviceMapping(Task<DeviceManager> deviceManager, Task<IpcListener?> ipcListener)
    {
        _deviceManager = deviceManager;
        _ipcListener = ipcListener;
        InitializeComponent();
    }

    private void OnAuroraCommandReceived(object? sender, string e)
    {
        var words = e.Split(Constants.StringSplit);
        if (words.Length != 2)
        {
            return;
        }

        var command = words[0];
        var json = words[1];
        switch (command)
        {
            case DeviceCommands.RemappableDevices:
                var remappableDevices = ReadDevices(json);

                Dispatcher.BeginInvoke(() => LoadDevices(remappableDevices), DispatcherPriority.Loaded);
                break;
        }
    }

    private async void OnLoaded(object? sender, RoutedEventArgs e)
    {
        if (!IsVisible)
        {
            return;
        }
        var ipcListener = await _ipcListener;
        if (ipcListener != null)
        {
            ipcListener.AuroraCommandReceived += OnAuroraCommandReceived;
        }

        await ReloadDevices();
    }

    private async void OnUnloaded(object? sender, EventArgs e)
    {
        _config.Value.SaveConfig();
        
        var ipcListener = await _ipcListener;
        if (ipcListener != null)
        {
            ipcListener.AuroraCommandReceived -= OnAuroraCommandReceived;
        }
    }

    private async Task ReloadDevices()
    {
        var deviceManager = await _deviceManager;

        if (!await deviceManager.IsDeviceManagerUp())
        {
            _devices.Clear();
            RemappableDeviceList.Children.Clear();
            return;
        }
        
        await deviceManager.DevicesPipe.RequestRemappableDevices();
    }

    private void LoadDevices(CurrentDevices remappableDevices)
    {
        // clear current devices
        _devices.Clear();

        foreach (var remappableDevicesDevice in remappableDevices.Devices)
        {
            _devices.Add(remappableDevicesDevice);
        }

        Dispatcher.Invoke(() =>
        {
            RemappableDeviceList.Children.Clear();
            foreach (var device in _devices)
            {
                // create a new button for the ui
                var button = new Button
                {
                    Content = device.DeviceSummary
                };

                button.Click += async (_, _) =>
                {
                    for (var i = 0; i < RemappableDeviceList.Children.Count; i++)
                    {
                        if (RemappableDeviceList.Children[i] is Button dButton)
                            dButton.IsEnabled = true;
                    }

                    button.IsEnabled = false;
                    await DeviceSelect(device);
                };

                RemappableDeviceList.Children.Add(button);
            }
        }, DispatcherPriority.Loaded);
    }

    private static CurrentDevices ReadDevices(string json)
    {
        return JsonSerializer.Deserialize<CurrentDevices>(json) ?? new CurrentDevices([]);
    }

    private async Task DeviceSelect(RemappableDevice remappableDevice)
    {
        // Rebuild the key area
        RemappableDeviceKeys.Children.Clear();
        NotRemappableTextBlock.Visibility = remappableDevice.RemapEnabled ? Visibility.Collapsed : Visibility.Visible;
        SetAllNoneBtn.IsEnabled = remappableDevice.RemapEnabled;
        SetAllLogoBtn.IsEnabled = remappableDevice.RemapEnabled;

        await Task.Run(async () =>
        {
            _keys.Clear();
            
            var deviceRemap = GetDeviceRemap(remappableDevice);

            var keyControls = remappableDevice.RgbNetLeds
                .Select(CreateKeyControl(deviceRemap, remappableDevice.RemapEnabled))
                .Select(kc => kc.ContinueWith(AssociateKeyControl(remappableDevice, deviceRemap)));
            foreach (var keyControlTask in keyControls)
            {
                var keyControl = await await keyControlTask;
                _keys.Add(keyControl);
                Dispatcher.BeginInvoke(() =>
                {
                    RemappableDeviceKeys.Children.Add(keyControl);
                }, DispatcherPriority.Input);
            }
        });
    }

    private Func<LedId, Task<RgbNetKeyToDeviceKeyControl>> CreateKeyControl(DeviceRemap deviceRemap, bool remappable)
    {
        return led =>
        {
            var tcs = new TaskCompletionSource<RgbNetKeyToDeviceKeyControl>(TaskCreationOptions.RunContinuationsAsynchronously);

            Dispatcher.BeginInvoke(() => { tcs.SetResult(new RgbNetKeyToDeviceKeyControl(deviceRemap, led, !remappable)); }, DispatcherPriority.Loaded);

            return tcs.Task;
        };
    }

    private Func<Task<RgbNetKeyToDeviceKeyControl>, Task<RgbNetKeyToDeviceKeyControl>> AssociateKeyControl(RemappableDevice remappableDevice, DeviceRemap deviceRemap)
    {
        return async keyControlTask =>
        {
            var keyControl = await keyControlTask;
            var led = keyControl.Led;

            keyControl.BlinkCallback += async () =>
            {
                var deviceManager = await _deviceManager;
                if (!await deviceManager.IsDeviceManagerUp())
                {
                    return;
                }
                await deviceManager.DevicesPipe.BlinkRemappableKey(remappableDevice, led);
            };
            keyControl.DeviceKeyChanged += async (_, newKey) =>
            {
                if (newKey != null)
                {
                    deviceRemap.KeyMapper[led] = newKey.Value;
                }
                else
                {
                    deviceRemap.KeyMapper.Remove(led);
                }

                var deviceManager = await _deviceManager;
                if (!await deviceManager.IsDeviceManagerUp())
                {
                    return;
                }
                await deviceManager.DevicesPipe.RemapKey(remappableDevice.DeviceId, led, newKey);
            };

            return keyControl;
        };
    }

    private DeviceRemap GetDeviceRemap(RemappableDevice device)
    {
        var netConfigDevice = DeviceMappingConfig.Config.Devices
            .Find(netConfigDevice => netConfigDevice.Name.Equals(device.DeviceId));
        if (netConfigDevice != null)
        {
            return netConfigDevice;
        }

        var rgbNetConfigDevice = new DeviceRemap(device);
        DeviceMappingConfig.Config.Devices.Add(rgbNetConfigDevice);
        return rgbNetConfigDevice;
    }

    #region UI

    private async void ReloadButton_Click(object? sender, RoutedEventArgs e)
    {
        await ReloadDevices();
    }

    private void SetAllNone_Click(object? sender, RoutedEventArgs e)
    {
        foreach (var key in _keys)
            key.SetDeviceKey(DeviceKeys.NONE);
    }

    private void SetAllLogo_Click(object? sender, RoutedEventArgs e)
    {
        foreach (var key in _keys)
            key.SetDeviceKey(DeviceKeys.Peripheral_Logo);
    }

    #endregion
}