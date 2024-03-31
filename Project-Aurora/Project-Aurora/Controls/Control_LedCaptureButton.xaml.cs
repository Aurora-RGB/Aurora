using System;
using System.Linq;
using System.Windows;
using Common.Devices;

namespace AuroraRgb.Controls;

public class ButtonLedChangedEventArgs(DeviceKeys? led) : EventArgs
{
    public DeviceKeys? DeviceKey => led;
}

public partial class Control_LedCaptureButton
{
    public event EventHandler<ButtonLedChangedEventArgs>? DeviceKeyChanged;

    private DeviceKeys? _deviceKeys;

    public DeviceKeys? DeviceKey
    {
        get => _deviceKeys;
        set
        {
            _deviceKeys = value;
            DeviceKeyChanged?.Invoke(this, new ButtonLedChangedEventArgs(value));
            DeviceKeyButton.Content = value.ToString();
            DeviceKeyButton.IsEnabled = true;
        }
    }

    public Control_LedCaptureButton()
    {
        InitializeComponent();
    }

    private void DeviceKeyButton_OnClick(object? sender, RoutedEventArgs e)
    {
        Global.key_recorder.StopRecording();
        Global.key_recorder.FinishedRecording += KeyRemapped;
        Global.key_recorder.StartRecording("DeviceRemap", true);
    }

    private void KeyRemapped(DeviceKeys[] keys)
    {
        Global.key_recorder.FinishedRecording -= KeyRemapped;
        if (keys.Length == 0)
        {
            return;
        }

        var assignedKey = keys[0];

        _deviceKeys = assignedKey;
        DeviceKeyButton.Content = assignedKey.ToString();

        DeviceKeyChanged?.Invoke(this, new ButtonLedChangedEventArgs(assignedKey));
        Global.key_recorder.Reset();
    }

    private void Control_LedCaptureButton_OnUnloaded(object sender, RoutedEventArgs e)
    {
        Global.key_recorder.FinishedRecording -= KeyRemapped;
        Global.key_recorder.StopRecording();
    }
}