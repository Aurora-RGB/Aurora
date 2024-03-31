using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using AuroraRgb.Controls;
using Common.Devices;
using Common.Devices.RGBNet;
using RGB.NET.Core;

namespace AuroraRgb.Devices.RGBNet.Config;

/// <summary>
/// Interaction logic for AsusKeyToDeviceKeyControl.xaml
/// </summary>
public partial class RgbNetKeyToDeviceKeyControl
{
    public Func<Task>? BlinkCallback { get; set; }
    private bool Disabled { get; }

    public void SetDeviceKey(DeviceKeys? value) => LedCapturer.DeviceKey = value;

    private readonly DeviceRemap _configDeviceRemap;
    public LedId Led { get; }

    public event EventHandler<DeviceKeys?>? DeviceKeyChanged;

    public RgbNetKeyToDeviceKeyControl(DeviceRemap configDeviceRemap, LedId led, bool disabled)
    {
        _configDeviceRemap = configDeviceRemap;
        Led = led;
        Disabled = disabled;
        
        InitializeComponent();

        KeyIdValue.Text = led.ToString();

        UpdateMappedLedId();
        LedCapturer.DeviceKeyChanged += DeviceKeyButton_OnClick;
    }

    private void UpdateMappedLedId()
    {
        LedCapturer.DeviceKeyChanged -= DeviceKeyButton_OnClick;
        if (_configDeviceRemap.KeyMapper.TryGetValue(Led, out var deviceKey))
        {
            LedCapturer.DeviceKey = deviceKey;
            ButtonBorder.BorderBrush = Brushes.Blue;
        }
        else
        {
            LedCapturer.DeviceKey = RgbNetKeyMappings.KeyNames.GetValueOrDefault(Led, DeviceKeys.NONE);
            ButtonBorder.BorderBrush = RgbNetKeyMappings.KeyNames.TryGetValue(Led, out _) ? Brushes.Black : Brushes.Red;
        }
        LedCapturer.DeviceKeyChanged += DeviceKeyButton_OnClick;
    }

    private void TestBlink(object? sender, RoutedEventArgs e)
    {
        BlinkCallback?.Invoke();
    }

    private void Clear(object? sender, RoutedEventArgs e)
    {
        DeviceKeyChanged?.Invoke(this, null);
        UpdateMappedLedId();
    }

    private void DeviceKeyButton_OnClick(object? sender, ButtonLedChangedEventArgs ledChangedEvent)
    {
        if (Disabled)
        {
            return;
        }

        DeviceKeyChanged?.Invoke(this, ledChangedEvent.DeviceKey);
        UpdateMappedLedId();
    }
}