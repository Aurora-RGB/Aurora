using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
using AuroraRgb.Devices;
using AuroraRgb.Modules.GameStateListen;
using AuroraRgb.Settings;
using AuroraRgb.Utils;
using Common;
using Common.Devices;
using Common.Devices.RGBNet;

namespace AuroraRgb.Controls;

public sealed partial class Control_DeviceCalibration : IDisposable
{
    private readonly Task<DeviceManager> _deviceManager;

    private readonly TransparencyComponent _transparencyComponent;
    private readonly Task<IpcListener?> _ipcListener;
    private readonly SemaphoreSlim _devicesUpdated = new(0);

    private Task<DeviceConfig>? _loadDeviceConfig;
    private IReadOnlyList<RemappableDevice> _rgbNetDevices = [];

    public Control_DeviceCalibration(Task<DeviceManager> deviceManager, Task<IpcListener?> ipcListener)
    {
        _deviceManager = deviceManager;
        _ipcListener = ipcListener;

        InitializeComponent();

        _transparencyComponent = new TransparencyComponent(this, null);
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
                LoadDevices(json);
                break;
        }
    }

    private void LoadDevices(string json)
    {
        Dispatcher.BeginInvoke(() => DeviceList.Children.Clear(), DispatcherPriority.Loaded);
        var remappableDevices = ReadDevices(json);
        var loadDeviceConfig = ConfigManager.LoadDeviceConfig();
        _loadDeviceConfig = loadDeviceConfig;
        _rgbNetDevices = remappableDevices.Devices.Where(d => d.RgbNetLeds.Count > 0).ToList();

        foreach (var device in remappableDevices.Devices)
        {
            var calibration = device.Calibration;
            Dispatcher.BeginInvoke(async () =>
            {
                var calibrationItem = new Control_DeviceCalibrationItem(await _deviceManager, await loadDeviceConfig, device, calibration);
                DeviceList.Children.Add(calibrationItem);
            }, DispatcherPriority.Loaded);
        }

        Unloaded += OnUnloaded;

        //release lock
        if (_devicesUpdated.CurrentCount > 0)
        {
            _devicesUpdated.Release(_devicesUpdated.CurrentCount);
        }

        async void OnUnloaded(object sender, RoutedEventArgs e)
        {
            await ConfigManager.SaveAsync(await loadDeviceConfig);
        }
    }

    private static CurrentDevices ReadDevices(string json)
    {
        return JsonSerializer.Deserialize<CurrentDevices>(json) ?? new CurrentDevices([]);
    }

    private async Task RefreshLists()
    {
        var deviceManager = await _deviceManager;
        if (!await deviceManager.IsDeviceManagerUp())
        {
            DeviceList.Children.Clear();
            return;
        }
        
        await deviceManager.DevicesPipe.RequestRemappableDevices();
        await _devicesUpdated.WaitAsync();
    }

    private async void Control_DeviceCalibration_OnLoaded(object sender, RoutedEventArgs e)
    {
        var ipcListener = await _ipcListener;
        if (ipcListener != null)
        {
            ipcListener.AuroraCommandReceived -= OnAuroraCommandReceived;
            ipcListener.AuroraCommandReceived += OnAuroraCommandReceived;
        }

        await RefreshLists();
    }

    private async void WizardButton_OnClick(object sender, RoutedEventArgs e)
    {
        if (_loadDeviceConfig is null || _rgbNetDevices.Count < 2)
        {
            MessageBox.Show("Assisted calibration needs at least two RGB.NET devices (a reference and a target).",
                "Assisted Calibration", MessageBoxButton.OK, MessageBoxImage.Information);
            return;
        }

        var wizard = new Control_CalibrationWizard(await _deviceManager, await _loadDeviceConfig, _rgbNetDevices)
        {
            Owner = GetWindow(this)
        };
        wizard.ShowDialog();
    }

    public void Dispose()
    {
        _transparencyComponent.Dispose();

        var ipcListener = _ipcListener.Result;
        if (ipcListener != null)
        {
            ipcListener.AuroraCommandReceived -= OnAuroraCommandReceived;
        }
    }
}