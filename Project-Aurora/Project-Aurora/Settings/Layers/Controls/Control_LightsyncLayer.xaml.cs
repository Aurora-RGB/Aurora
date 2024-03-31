using System.Collections.Generic;
using System.Windows;
using System.Windows.Data;
using Common.Devices;

namespace AuroraRgb.Settings.Layers.Controls;

public partial class Control_LightsyncLayer
{
    private bool _settingsSet;
    private LogitechLayerHandler? Context => DataContext as LogitechLayerHandler;

    public Control_LightsyncLayer()
    {
        InitializeComponent();
    }

    public Control_LightsyncLayer(LogitechLayerHandler dataContext)
    {
        InitializeComponent();

        DataContext = dataContext;
    }

    private void SetSettings()
    {
        if (Context == null || _settingsSet) return;

        ColorPostProcessCheckBox.IsChecked = Context.Properties.ColorPostProcessEnabled;
        BrightnessSlider.Value = Context.Properties.BrightnessChange;
        SaturationSlider.Value = Context.Properties.SaturationChange;
        HueSlider.Value = Context.Properties.HueShift;
        CollectionViewSource.GetDefaultView(KeyCloneListBox.ItemsSource).Refresh();
        _settingsSet = true;
    }

    private void OnUserControlLoaded(object? sender, RoutedEventArgs e)
    {
        SetSettings();
        Loaded -= OnUserControlLoaded;
    }

    private void OnAddKeyCloneButtonClick(object? sender, RoutedEventArgs e)
    {
        var source = KeyCloneSourceButton.DeviceKey;
        var target = KeyCloneTargetButton.DeviceKey;
        if (source == null || target == null)
            return;

        var sourceKey = source.GetValueOrDefault();
        var destKey = target.GetValueOrDefault();

        if (sourceKey == destKey)
            return;

        if (Context == null)
        {
            return;
        }
        var cloneMap = Context.Properties.KeyCloneMap;
        if (!cloneMap.TryAdd(destKey, sourceKey))
            return;

        KeyCloneTargetButton.DeviceKey = null;
        CollectionViewSource.GetDefaultView(KeyCloneListBox.ItemsSource).Refresh();
    }

    private void OnDeleteKeyCloneButtonClick(object? sender, RoutedEventArgs e)
    {
        if (Context == null)
        {
            return;
        }
        var cloneMap = Context.Properties.KeyCloneMap;
        foreach (var o in KeyCloneListBox.SelectedItems)
        {
            if (o is not KeyValuePair<DeviceKeys, DeviceKeys> (var source, var target)) continue;
            if (!cloneMap.TryGetValue(source, out var key) || key != target)
                continue;

            cloneMap.Remove(source);
        }

        CollectionViewSource.GetDefaultView(KeyCloneListBox.ItemsSource).Refresh();
    }
}