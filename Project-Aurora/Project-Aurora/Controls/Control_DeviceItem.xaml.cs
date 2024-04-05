using System;
using System.Threading.Tasks;
using System.Timers;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;
using AuroraRgb.Devices;
using AuroraRgb.Modules;
using AuroraRgb.Settings;
using AuroraRgb.Settings.Controls;
using Common.Devices;

namespace AuroraRgb.Controls;

/// <summary>
/// Interaction logic for Control_DeviceItem.xaml
/// </summary>
public partial class Control_DeviceItem
{
    private readonly DeviceContainer _device;
    private readonly DeviceConfig _deviceConfigs;

    private readonly Timer _updateControlsTimer = new(1000);

    private bool _deviceRunning;

    private string? _sdkLink;

    public Control_DeviceItem(DeviceConfig deviceConfigs, DeviceContainer device)
    {
        _deviceConfigs = deviceConfigs;
        _device = device;

        InitializeComponent();
    }

    private void Update_controls_timer_Elapsed(object? sender, EventArgs e)
    {
        if (IsVisible)
        {
            RequestUpdate();
        }
    }

    private void RequestUpdate()
    {
        Dispatcher.BeginInvoke(() =>
        {
            var memorySharedDevice = _device.Device;
            Task.Run(() => { memorySharedDevice.RequestUpdate(); });
        }, DispatcherPriority.Background);
    }

    private void btnToggleOnOff_Click(object? sender, EventArgs e)
    {
        BtnStart.Content = "Working...";
        BtnStart.IsEnabled = false;
        var device = _device;
        if (device.Device.IsInitialized || device.Device.IsDoingWork)
        {
            Task.Run(async () =>
            {
                await device.DisableDevice();
            });
        }
        else
        {
            Task.Run(async () =>
            {
                await device.EnableDevice();
            });
        }
    }

    private void btnToggleEnableDisable_Click(object? sender, EventArgs e)
    {
        var deviceEnabled = !_deviceConfigs.EnabledDevices.Contains(_device.Device.DeviceName);
        if (deviceEnabled)
        {
            _deviceConfigs.EnabledDevices.Add(_device.Device.DeviceName);
            var device = _device;
            Task.Run(async () =>
            {
                await device.EnableDevice();
            });
        }
        else
        {
            _deviceConfigs.EnabledDevices.Remove(_device.Device.DeviceName);
        }
        UpdateDynamic();

        if (_deviceRunning)
        {
            BtnStart.Content = "Working...";
            BtnStart.IsEnabled = false;
            BtnEnable.IsEnabled = false;
            
            var device = _device;
            Task.Run(async () =>
            {
                await device.DisableDevice();
            });
        }
        Task.Run(() =>
        {
            ConfigManager.Save(_deviceConfigs);
        });
    }

    private void UserControl_Loaded(object? sender, EventArgs e)
    {
        if (!IsVisible)
        {
            return;
        }
        _device.Device.Updated += OnDeviceOnUpdated;
        
        Dispatcher.BeginInvoke(UpdateStatic, DispatcherPriority.Loaded);
        Dispatcher.BeginInvoke(UpdateDynamic, DispatcherPriority.DataBind);

        _updateControlsTimer.Elapsed += Update_controls_timer_Elapsed;
        _updateControlsTimer.Start();
    }

    private void Control_DeviceItem_OnUnloaded(object? sender, EventArgs e)
    {
        _device.Device.Updated -= OnDeviceOnUpdated;

        _updateControlsTimer.Stop();
        _updateControlsTimer.Elapsed -= Update_controls_timer_Elapsed;
    }

    private void OnDeviceOnUpdated(object? o, EventArgs eventArgs)
    {
        if (IsVisible)
        {
            Dispatcher.BeginInvoke(UpdateDynamic, DispatcherPriority.DataBind);
        }
    }

    private void UpdateStatic()
    {
        DeviceName.Text = _device.Device.DeviceName;
        BtnOptions.IsEnabled = _device.Device.RegisteredVariables.Count != 0;

        if (!OnlineSettings.DeviceTooltips.TryGetValue(_device.Device.DeviceName, out var tooltips))
        {
            return;
        }
        
        Beta.Visibility = tooltips.Beta ? Visibility.Visible : Visibility.Hidden;

        var infoTooltip = tooltips.Info;
        if (infoTooltip != null)
        {
            InfoTooltip.HintTooltip = infoTooltip;
            InfoTooltip.Visibility = Visibility.Visible;
        }
        else
        {
            InfoTooltip.Visibility = Visibility.Hidden;
        }

        _sdkLink = tooltips.SdkLink;
        if (_sdkLink != null)
        {
            SdkLink.MouseDoubleClick -= SdkLink_Clicked;
            SdkLink.MouseDoubleClick += SdkLink_Clicked;
            SdkLink.Visibility = Visibility.Visible;
        }
        else
        {
            SdkLink.MouseDoubleClick -= SdkLink_Clicked;
            SdkLink.Visibility = Visibility.Hidden;
        }

        Recommended.Visibility = tooltips.Recommended ? Visibility.Visible : Visibility.Hidden;
    }

    private void UpdateDynamic()
    {
        if (_device.Device.IsDoingWork)
        {
            _deviceRunning = false;
            BtnStart.Content = "Cancel";
            BtnStart.IsEnabled = true;
            BtnEnable.IsEnabled = false;
        }
        else if (_device.Device.IsInitialized)
        {
            _deviceRunning = true;
            BtnStart.Content = "Stop";
            BtnStart.IsEnabled = true;
            BtnEnable.IsEnabled = true;
        }
        else
        {
            _deviceRunning = false;
            BtnStart.Content = "Start";
            BtnStart.IsEnabled = true;
            BtnEnable.IsEnabled = true;
        }

        DeviceDetails.Text = _device.Device.DeviceDetails;
        DevicePerformance.Text = _device.Device.DeviceUpdatePerformance;

        if (!_deviceConfigs.EnabledDevices.Contains(_device.Device.DeviceName))
        {
            BtnEnable.Content = "Enable";
            BtnStart.IsEnabled = false;
        }
        else if (!_device.Device.IsDoingWork)
        {
            BtnEnable.Content = "Disable";
        }
    }

    private void SdkLink_Clicked(object? sender, MouseButtonEventArgs e)
    {
        if (_sdkLink != null)
        {
            System.Diagnostics.Process.Start("explorer", _sdkLink);
        }
    }

    private void btnViewOptions_Click(object? sender, RoutedEventArgs e)
    {
        var optionsWindow = new Window_VariableRegistryEditor(_deviceConfigs)
        {
            Title = $"{_device.Device.DeviceName} - Options",
            SizeToContent = SizeToContent.WidthAndHeight,
            VarRegistryEditor =
            {
                RegisteredVariables = _device.Device.RegisteredVariables
            }
        };
        optionsWindow.Closing += (_, _) => { ConfigManager.Save(_deviceConfigs); };

        optionsWindow.ShowDialog();
    }
}