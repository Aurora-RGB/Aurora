using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using AuroraRgb.Devices;
using AuroraRgb.Modules.GameStateListen;
using AuroraRgb.Settings;
using Common.Devices;

namespace AuroraRgb.Controls;

/// <summary>
/// Interaction logic for Control_DeviceManager.xaml
/// </summary>
public partial class Control_DeviceManager
{
    private readonly Task<DeviceManager> _deviceManager;
    private readonly Task<IpcListener?> _ipcListener;

    private CancellationTokenSource _updateCancelTokenSource = new();

    public Control_DeviceManager(Task<DeviceManager> deviceManager, Task<IpcListener?> ipcListener)
    {
        _deviceManager = deviceManager;
        _ipcListener = ipcListener;

        InitializeComponent();
    }

    private async void Control_DeviceManager_Loaded(object? sender, RoutedEventArgs e)
    {
        if (!IsVisible)
        {
            return;
        }

        await FirstInit();
    }

    private async Task FirstInit()
    {
        var deviceConfig = await ConfigManager.LoadDeviceConfig();
        var deviceManager = await _deviceManager;

        await Task.Run(() => UpdateControls(deviceConfig, deviceManager.DeviceContainers)).ConfigureAwait(false);

        var deviceManagerUpdatedHandle = DeviceManagerOnDevicesUpdated();
        deviceManager.DevicesUpdated -= deviceManagerUpdatedHandle;
        deviceManager.DevicesUpdated += deviceManagerUpdatedHandle;

        Unloaded += (_, _) => deviceManager.DevicesUpdated -= deviceManagerUpdatedHandle;

        EventHandler<DevicesUpdatedEventArgs> DeviceManagerOnDevicesUpdated()
        {
            return ManagerOnDevicesUpdated;

            async void ManagerOnDevicesUpdated(object? _, DevicesUpdatedEventArgs eventArgs)
            {
                await UpdateControls(deviceConfig, eventArgs.DeviceContainers);
            }
        }
    }

    private async Task UpdateControls(DeviceConfig deviceConfig, IList<DeviceContainer> deviceContainers)
    {
        var cancelSource = new CancellationTokenSource();
        await _updateCancelTokenSource.CancelAsync();
        
        var deviceManager = await _deviceManager;
        var isDeviceManagerUp = await deviceManager.IsDeviceManagerUp();
        if (isDeviceManagerUp && deviceContainers.Count == 0)
        {
            return;
        }
        await Dispatcher.InvokeAsync(() =>
        {
            var oldToken = _updateCancelTokenSource;
            _updateCancelTokenSource = cancelSource;

            oldToken.Dispose();

            LstDevices.Children.Clear();
            NoDevManTextBlock.Visibility = isDeviceManagerUp ? Visibility.Collapsed : Visibility.Visible;
            PopulateDevices(deviceConfig, deviceContainers, cancelSource.Token);
        }, DispatcherPriority.Loaded);
    }

    private void PopulateDevices(DeviceConfig deviceConfig, IEnumerable<DeviceContainer> deviceContainers, CancellationToken cancelSourceToken)
    {
        foreach (var deviceContainer in deviceContainers)
        {
            Dispatcher.InvokeAsync(() =>
            {
                var controlDeviceItem = new Control_DeviceItem(deviceConfig, deviceContainer);
                var listViewItem = new ListViewItem
                {
                    Content = controlDeviceItem,
                };
                LstDevices.Children.Add(listViewItem);
            }, DispatcherPriority.Loaded, cancelSourceToken);
        }
    }

    private async void btnRestartAll_Click(object? sender, RoutedEventArgs e)
    {
        var devManager = await _deviceManager;
        await devManager.ShutdownDevices();
        await devManager.InitializeDevices();
    }

    private void btnCalibrate_Click(object? sender, RoutedEventArgs e)
    {
        var calibration = new Control_DeviceCalibration(_deviceManager, _ipcListener);
        calibration.Show();
    }
}