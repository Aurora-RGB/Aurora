﻿using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using Aurora.Modules.Inputs;
using Aurora.Utils;
using Common.Devices;
using Xceed.Wpf.Toolkit;
using MessageBox = System.Windows.MessageBox;

namespace Aurora.Profiles.Dota_2.Layers;

/// <summary>
/// Interaction logic for Control_Dota2ItemLayer.xaml
/// </summary>
public partial class Control_Dota2ItemLayer
{
    private bool _settingsSet;

    public Control_Dota2ItemLayer()
    {
        InitializeComponent();
    }

    public Control_Dota2ItemLayer(Dota2ItemLayerHandler dataContext)
    {
        InitializeComponent();

        DataContext = dataContext;
    }

    private void SetSettings()
    {
        if (DataContext is not Dota2ItemLayerHandler || _settingsSet) return;

        ColorPicker_Item_Empty.SelectedColor = ColorUtils.DrawingColorToMediaColor(((Dota2ItemLayerHandler)DataContext).Properties._EmptyItemColor ?? System.Drawing.Color.Empty);
        ColorPicker_Item_Cooldown.SelectedColor = ColorUtils.DrawingColorToMediaColor(((Dota2ItemLayerHandler)DataContext).Properties._ItemCooldownColor ?? System.Drawing.Color.Empty);
        ColorPicker_Item_NoCharges.SelectedColor = ColorUtils.DrawingColorToMediaColor(((Dota2ItemLayerHandler)DataContext).Properties._ItemNoChargersColor ?? System.Drawing.Color.Empty);
        ColorPicker_Item_Color.SelectedColor = ColorUtils.DrawingColorToMediaColor(((Dota2ItemLayerHandler)DataContext).Properties._ItemsColor ?? System.Drawing.Color.Empty);
        CheckBox_Use_Item_Colors.IsChecked = ((Dota2ItemLayerHandler)DataContext).Properties._UseItemColors;

        UIUtils.SetSingleKey(item_slot1_textblock, ((Dota2ItemLayerHandler)DataContext).Properties._ItemKeys, 0);
        UIUtils.SetSingleKey(item_slot2_textblock, ((Dota2ItemLayerHandler)DataContext).Properties._ItemKeys, 1);
        UIUtils.SetSingleKey(item_slot3_textblock, ((Dota2ItemLayerHandler)DataContext).Properties._ItemKeys, 2);
        UIUtils.SetSingleKey(item_slot4_textblock, ((Dota2ItemLayerHandler)DataContext).Properties._ItemKeys, 3);
        UIUtils.SetSingleKey(item_slot5_textblock, ((Dota2ItemLayerHandler)DataContext).Properties._ItemKeys, 4);
        UIUtils.SetSingleKey(item_slot6_textblock, ((Dota2ItemLayerHandler)DataContext).Properties._ItemKeys, 5);
        UIUtils.SetSingleKey(item_slot7_textblock, ((Dota2ItemLayerHandler)DataContext).Properties._ItemKeys, 6);
        UIUtils.SetSingleKey(item_slot8_textblock, ((Dota2ItemLayerHandler)DataContext).Properties._ItemKeys, 7);
        UIUtils.SetSingleKey(item_slot9_textblock, ((Dota2ItemLayerHandler)DataContext).Properties._ItemKeys, 8);
        UIUtils.SetSingleKey(stash_slot1_textblock, ((Dota2ItemLayerHandler)DataContext).Properties._ItemKeys, 9);
        UIUtils.SetSingleKey(stash_slot2_textblock, ((Dota2ItemLayerHandler)DataContext).Properties._ItemKeys, 10);
        UIUtils.SetSingleKey(stash_slot3_textblock, ((Dota2ItemLayerHandler)DataContext).Properties._ItemKeys, 11);
        UIUtils.SetSingleKey(stash_slot4_textblock, ((Dota2ItemLayerHandler)DataContext).Properties._ItemKeys, 12);
        UIUtils.SetSingleKey(stash_slot5_textblock, ((Dota2ItemLayerHandler)DataContext).Properties._ItemKeys, 13);
        UIUtils.SetSingleKey(stash_slot6_textblock, ((Dota2ItemLayerHandler)DataContext).Properties._ItemKeys, 14);

        _settingsSet = true;
    }

    private void UserControl_Loaded(object? sender, RoutedEventArgs e)
    {
        SetSettings();

        Loaded -= UserControl_Loaded;
    }

    private void item_slot1_textblock_MouseDown(object? sender, MouseButtonEventArgs e)
    {
        RecordSingleKey("Dota 2 - Item Slot 1 Key", (TextBlock)sender, item1_keys_callback);
    }

    private void item1_keys_callback(DeviceKeys[] resultingKeys)
    {
        ChangeKeybind(resultingKeys, item_slot1_textblock, 0);
    }

    private void item_slot2_textblock_MouseDown(object? sender, MouseButtonEventArgs e)
    {
        RecordSingleKey("Dota 2 - Item Slot 2 Key", (TextBlock)sender, item2_keys_callback);
    }

    private void item2_keys_callback(DeviceKeys[] resultingKeys)
    {
        ChangeKeybind(resultingKeys, item_slot2_textblock, 1);
    }

    private void item_slot3_textblock_MouseDown(object? sender, MouseButtonEventArgs e)
    {
        RecordSingleKey("Dota 2 - Item Slot 3 Key", (TextBlock)sender, item3_keys_callback);
    }

    private void item3_keys_callback(DeviceKeys[] resultingKeys)
    {
        ChangeKeybind(resultingKeys, item_slot3_textblock, 2);
    }

    private void item_slot4_textblock_MouseDown(object? sender, MouseButtonEventArgs e)
    {
        RecordSingleKey("Dota 2 - Item Slot 4 Key", (TextBlock)sender, item4_keys_callback);
    }

    private void item4_keys_callback(DeviceKeys[] resultingKeys)
    {
        ChangeKeybind(resultingKeys, item_slot4_textblock, 3);
    }

    private void item_slot5_textblock_MouseDown(object? sender, MouseButtonEventArgs e)
    {
        RecordSingleKey("Dota 2 - Item Slot 5 Key", (TextBlock)sender, item5_keys_callback);
    }

    private void item5_keys_callback(DeviceKeys[] resultingKeys)
    {
        ChangeKeybind(resultingKeys, item_slot5_textblock, 4);
    }

    private void item_slot6_textblock_MouseDown(object? sender, MouseButtonEventArgs e)
    {
        RecordSingleKey("Dota 2 - Item Slot 6 Key", (TextBlock)sender, item6_keys_callback);
    }

    private void item6_keys_callback(DeviceKeys[] resultingKeys)
    {
        ChangeKeybind(resultingKeys, item_slot6_textblock, 5);
    }


    private void item_slot7_textblock_MouseDown(object? sender, MouseButtonEventArgs e)
    {
        RecordSingleKey("Dota 2 - Item Slot 7 Key", (TextBlock)sender, item7_keys_callback);
    }

    private void item7_keys_callback(DeviceKeys[] resultingKeys)
    {
        ChangeKeybind(resultingKeys, item_slot7_textblock, 6);
    }

    private void item_slot8_textblock_MouseDown(object? sender, MouseButtonEventArgs e)
    {
        RecordSingleKey("Dota 2 - Item Slot 8 Key", (TextBlock)sender, item8_keys_callback);
    }

    private void item8_keys_callback(DeviceKeys[] resultingKeys)
    {
        ChangeKeybind(resultingKeys, item_slot8_textblock, 7);
    }

    private void item_slot9_textblock_MouseDown(object? sender, MouseButtonEventArgs e)
    {
        RecordSingleKey("Dota 2 - Item Slot 9 Key", (TextBlock)sender, item9_keys_callback);
    }

    private void item9_keys_callback(DeviceKeys[] resultingKeys)
    {
        ChangeKeybind(resultingKeys, item_slot9_textblock, 8);
    }

    private void stash_slot1_textblock_MouseDown(object? sender, MouseButtonEventArgs e)
    {
        RecordSingleKey("Dota 2 - Stash Slot 1 Key", (TextBlock)sender, stash1_keys_callback);
    }

    private void stash1_keys_callback(DeviceKeys[] resultingKeys)
    {
        ChangeKeybind(resultingKeys, stash_slot1_textblock, 9);
    }

    private void stash_slot2_textblock_MouseDown(object? sender, MouseButtonEventArgs e)
    {
        RecordSingleKey("Dota 2 - Stash Slot 2 Key", (TextBlock)sender, stash2_keys_callback);
    }

    private void stash2_keys_callback(DeviceKeys[] resultingKeys)
    {
        ChangeKeybind(resultingKeys, stash_slot2_textblock, 10);
    }

    private void stash_slot3_textblock_MouseDown(object? sender, MouseButtonEventArgs e)
    {
        RecordSingleKey("Dota 2 - Stash Slot 3 Key", (TextBlock)sender, stash3_keys_callback);
    }

    private void stash3_keys_callback(DeviceKeys[] resultingKeys)
    {
        ChangeKeybind(resultingKeys, stash_slot3_textblock, 11);
    }

    private void stash_slot4_textblock_MouseDown(object? sender, MouseButtonEventArgs e)
    {
        RecordSingleKey("Dota 2 - Stash Slot 4 Key", (TextBlock)sender, stash4_keys_callback);
    }

    private void stash4_keys_callback(DeviceKeys[] resultingKeys)
    {
        ChangeKeybind(resultingKeys, stash_slot4_textblock, 12);
    }

    private void stash_slot5_textblock_MouseDown(object? sender, MouseButtonEventArgs e)
    {
        RecordSingleKey("Dota 2 - Stash Slot 5 Key", (TextBlock)sender, stash5_keys_callback);
    }

    private void stash5_keys_callback(DeviceKeys[] resultingKeys)
    {
        ChangeKeybind(resultingKeys, stash_slot5_textblock, 13);
    }

    private void stash_slot6_textblock_MouseDown(object? sender, MouseButtonEventArgs e)
    {
        RecordSingleKey("Dota 2 - Stash Slot 6 Key", (TextBlock)sender, stash6_keys_callback);
    }

    private void stash6_keys_callback(DeviceKeys[] resultingKeys)
    {
        ChangeKeybind(resultingKeys, stash_slot6_textblock, 14);
    }

    private void ChangeKeybind(IReadOnlyList<DeviceKeys> resultingKeys, TextBlock textBlock, int itemPosition)
    {
        Dispatcher.Invoke(() =>
        {
            textBlock.Background = new SolidColorBrush(Color.FromArgb(0, 0, 0, 0));

            if (resultingKeys.Count <= 0) return;
            if (IsLoaded)
                ((Dota2ItemLayerHandler)DataContext).Properties._ItemKeys[itemPosition] = resultingKeys[0];

            UIUtils.SetSingleKey(textBlock, ((Dota2ItemLayerHandler)DataContext).Properties._ItemKeys, itemPosition);
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
                callback(resultingKeys);
                Global.key_recorder.Reset();
            }

            Global.key_recorder.StartRecording(recorder, true);
            textBlock.Background = new SolidColorBrush(Color.FromArgb(255, 255, 0, 0));
        }
    }

    private void ColorPicker_Item_Empty_SelectedColorChanged(object? sender, RoutedPropertyChangedEventArgs<Color?> e)
    {
        if (IsLoaded && _settingsSet && DataContext is Dota2ItemLayerHandler && sender is ColorPicker { SelectedColor: not null } colorPicker)
            ((Dota2ItemLayerHandler)DataContext).Properties._EmptyItemColor = ColorUtils.MediaColorToDrawingColor(colorPicker.SelectedColor.Value);
    }

    private void ColorPicker_Item_Cooldown_SelectedColorChanged(object? sender, RoutedPropertyChangedEventArgs<Color?> e)
    {
        if (IsLoaded && _settingsSet && DataContext is Dota2ItemLayerHandler && sender is ColorPicker { SelectedColor: not null } colorPicker)
            ((Dota2ItemLayerHandler)DataContext).Properties._ItemCooldownColor = ColorUtils.MediaColorToDrawingColor(colorPicker.SelectedColor.Value);
    }

    private void ColorPicker_Item_NoCharges_SelectedColorChanged(object? sender, RoutedPropertyChangedEventArgs<Color?> e)
    {
        if (IsLoaded && _settingsSet && DataContext is Dota2ItemLayerHandler && sender is ColorPicker { SelectedColor: not null } colorPicker)
            ((Dota2ItemLayerHandler)DataContext).Properties._ItemNoChargersColor = ColorUtils.MediaColorToDrawingColor(colorPicker.SelectedColor.Value);
    }

    private void ColorPicker_Item_Color_SelectedColorChanged(object? sender, RoutedPropertyChangedEventArgs<Color?> e)
    {
        if (IsLoaded && _settingsSet && DataContext is Dota2ItemLayerHandler && sender is ColorPicker { SelectedColor: not null } colorPicker)
            ((Dota2ItemLayerHandler)DataContext).Properties._ItemsColor = ColorUtils.MediaColorToDrawingColor(colorPicker.SelectedColor.Value);
    }

    private void CheckBox_Use_Item_Colors_Checked(object? sender, RoutedEventArgs e)
    {
        if (IsLoaded && _settingsSet && DataContext is Dota2ItemLayerHandler && sender is CheckBox { IsChecked: not null } checkBox)
            ((Dota2ItemLayerHandler)DataContext).Properties._UseItemColors = checkBox.IsChecked.Value;
    }
}