using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using AuroraRgb.Modules.Inputs;
using AuroraRgb.Utils;
using Common.Devices;
using Xceed.Wpf.Toolkit;
using MessageBox = System.Windows.MessageBox;

namespace AuroraRgb.Profiles.Dota_2.Layers;

/// <summary>
/// Interaction logic for Control_Dota2AbilityLayer.xaml
/// </summary>
public partial class Control_Dota2AbilityLayer
{
    private bool _settingsSet;

    public Control_Dota2AbilityLayer()
    {
        InitializeComponent();
    }

    public Control_Dota2AbilityLayer(Dota2AbilityLayerHandler dataContext)
    {
        InitializeComponent();

        DataContext = dataContext;
    }

    private void SetSettings()
    {
        if (DataContext is not Dota2AbilityLayerHandler layerHandler || _settingsSet) return;

        ColorPicker_CanCastAbility.SelectedColor = ColorUtils.DrawingColorToMediaColor(layerHandler.Properties.CanCastAbilityColor);
        ColorPicker_CanNotCastAbility.SelectedColor = ColorUtils.DrawingColorToMediaColor(layerHandler.Properties.CanNotCastAbilityColor);
        UIUtils.SetSingleKey(ability_key1_textblock, layerHandler.Properties.AbilityKeys, 0);
        UIUtils.SetSingleKey(ability_key2_textblock, layerHandler.Properties.AbilityKeys, 1);
        UIUtils.SetSingleKey(ability_key3_textblock, layerHandler.Properties.AbilityKeys, 2);
        UIUtils.SetSingleKey(ability_key4_textblock, layerHandler.Properties.AbilityKeys, 3);
        UIUtils.SetSingleKey(ability_key5_textblock, layerHandler.Properties.AbilityKeys, 4);
        UIUtils.SetSingleKey(ability_key6_textblock, layerHandler.Properties.AbilityKeys, 5);

        _settingsSet = true;
    }

    private void UserControl_Loaded(object? sender, RoutedEventArgs e)
    {
        SetSettings();

        Loaded -= UserControl_Loaded;
    }

    private void abilities_canuse_colorpicker_SelectedColorChanged(object? sender, RoutedPropertyChangedEventArgs<Color?> e)
    {
        if (IsLoaded && _settingsSet && DataContext is Dota2AbilityLayerHandler layerHandler && sender is ColorPicker { SelectedColor: not null } colorPicker)
            layerHandler.Properties.CanCastAbilityColor = ColorUtils.MediaColorToDrawingColor(colorPicker.SelectedColor.Value);
    }

    private void abilities_cannotuse_colorpicker_SelectedColorChanged(object? sender, RoutedPropertyChangedEventArgs<Color?> e)
    {
        if (IsLoaded && _settingsSet && DataContext is Dota2AbilityLayerHandler layerHandler && sender is ColorPicker { SelectedColor: not null } colorPicker)
            layerHandler.Properties.CanNotCastAbilityColor = ColorUtils.MediaColorToDrawingColor(colorPicker.SelectedColor.Value);
    }

    private void ability_key1_textblock_MouseDown(object? sender, MouseButtonEventArgs e)
    {
        RecordSingleKey("Dota 2 - Ability 1 Key", (TextBlock)sender, ability1_keys_callback);
    }

    private void ability1_keys_callback(DeviceKeys[] resultingKeys)
    {
        ChangeKeybind(resultingKeys, ability_key1_textblock, 0);
    }

    private void ability_key2_textblock_MouseDown(object? sender, MouseButtonEventArgs e)
    {
        RecordSingleKey("Dota 2 - Ability 2 Key", (TextBlock)sender, ability2_keys_callback);
    }

    private void ability2_keys_callback(DeviceKeys[] resultingKeys)
    {
        ChangeKeybind(resultingKeys, ability_key2_textblock, 1);
    }

    private void ability_key3_textblock_MouseDown(object? sender, MouseButtonEventArgs e)
    {
        RecordSingleKey("Dota 2 - Ability 3 Key", (TextBlock)sender, ability3_keys_callback);
    }

    private void ability3_keys_callback(DeviceKeys[] resultingKeys)
    {
        ChangeKeybind(resultingKeys, ability_key3_textblock, 2);
    }

    private void ability_key4_textblock_MouseDown(object? sender, MouseButtonEventArgs e)
    {
        RecordSingleKey("Dota 2 - Ability 4 Key", (TextBlock)sender, ability4_keys_callback);
    }

    private void ability4_keys_callback(DeviceKeys[] resultingKeys)
    {
        ChangeKeybind(resultingKeys, ability_key4_textblock, 3);
    }

    private void ability_key5_textblock_MouseDown(object? sender, MouseButtonEventArgs e)
    {
        RecordSingleKey("Dota 2 - Ability 5 Key", (TextBlock)sender, ability5_keys_callback);
    }

    private void ability5_keys_callback(DeviceKeys[] resultingKeys)
    {
        ChangeKeybind(resultingKeys, ability_key5_textblock, 4);
    }

    private void ability_key6_textblock_MouseDown(object? sender, MouseButtonEventArgs e)
    {
        RecordSingleKey("Dota 2 - Ultimate Ability Key", (TextBlock)sender, ability6_keys_callback);
    }

    private void ability6_keys_callback(DeviceKeys[] resultingKeys)
    {
        ChangeKeybind(resultingKeys, ability_key6_textblock, 5);
    }

    private void ChangeKeybind(IReadOnlyList<DeviceKeys> resultingKeys, TextBlock textBlock, int abilityPosition)
    {
        Dispatcher.Invoke(() =>
        {
            textBlock.Background = new SolidColorBrush(Color.FromArgb(0, 0, 0, 0));

            if (resultingKeys.Count <= 0) return;
            if (IsLoaded)
                ((Dota2AbilityLayerHandler)DataContext).Properties.AbilityKeys[abilityPosition] = resultingKeys[0];

            UIUtils.SetSingleKey(textBlock, ((Dota2AbilityLayerHandler)DataContext).Properties.AbilityKeys, abilityPosition);
        });
    }

    private void RecordSingleKey(string recorder, TextBlock textBlock, KeyRecorder.RecordingFinishedHandler callback)
    {
        if (Global.key_recorder.IsRecording())
        {
            if (Global.key_recorder.GetRecordingType().Equals(recorder))
            {
                Global.key_recorder.StopRecording();

                Global.key_recorder.Reset();
            }
            else
            {
                MessageBox.Show("You are already recording a key sequence for " + Global.key_recorder.GetRecordingType());
            }
        }
        else
        {
            Global.key_recorder.FinishedRecording += KeyRecorderOnFinishedRecording;
            void KeyRecorderOnFinishedRecording(DeviceKeys[] resultingKeys)
            {
                Global.key_recorder.FinishedRecording -= KeyRecorderOnFinishedRecording;
                if (resultingKeys.Length == 0)
                {
                    return;
                }
                callback(resultingKeys);
                Global.key_recorder.Reset();
            }

            Global.key_recorder.StartRecording(recorder, true);
            textBlock.Background = new SolidColorBrush(Color.FromArgb(255, 255, 0, 0));
        }
    }
}