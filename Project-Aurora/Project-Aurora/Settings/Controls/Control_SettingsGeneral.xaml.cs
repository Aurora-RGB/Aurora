﻿using System;
using System.Windows;
using System.Windows.Controls;
using AuroraRgb.Modules;
using AuroraRgb.Utils;
using AuroraRgb.Utils.IpApi;
using Xceed.Wpf.Toolkit;

namespace AuroraRgb.Settings.Controls;

public partial class Control_SettingsGeneral
{
    public Control_SettingsGeneral()
    {
        InitializeComponent();

        TransparencyCheckbox.IsEnabled = TransparencyComponent.UseMica;
    }

    private void Control_SettingsGeneral_OnLoaded(object sender, RoutedEventArgs e)
    {
        DataContext = Global.Configuration;
        RunAtWinStartup.IsChecked = AutoStartUtils.GetAutostartTask(out var delayAmount);
        StartDelayAmount.Value = delayAmount;

        LocationPanel.Visibility = Visibility.Hidden;
        LocationRevealButton.Visibility = Visibility.Visible;
    }

    private void ExcludedAdd_Click(object? sender, RoutedEventArgs e)
    {
        var dialog = new Window_ProcessSelection { ButtonLabel = "Exclude Process" };
        if (dialog.ShowDialog() == true &&
            !string.IsNullOrWhiteSpace(dialog.ChosenExecutableName) &&
            !Global.Configuration.ExcludedPrograms.Contains(dialog.ChosenExecutableName)
           )
            Global.Configuration.ExcludedPrograms.Add(dialog.ChosenExecutableName);
    }

    private void ExcludedRemove_Click(object? sender, RoutedEventArgs e)
    {
        var selectedItem = ExcludedProcessesList.SelectedItem as string;
        if (!string.IsNullOrEmpty(selectedItem))
            Global.Configuration.ExcludedPrograms.Remove(selectedItem);
    }

    private void RunAtWinStartup_Checked(object? sender, RoutedEventArgs e)
    {
        if (!IsLoaded || sender is not CheckBox checkBox) return;
        AutoStartUtils.SetEnabled(checkBox.IsChecked.GetValueOrDefault());
    }

    private void HighPriorityCheckbox_Checked(object? sender, EventArgs e)
    {
        if (!IsLoaded) return;
        PerformanceModeModule.UpdatePriority();
    }

    private void StartDelayAmount_ValueChanged(object? sender, RoutedPropertyChangedEventArgs<object> e)
    {
        if (!IsLoaded || sender is not IntegerUpDown integerUpDown) return;
        AutoStartUtils.SetStartupDelay(integerUpDown.Value ?? 0);
    }

    private async void ResetLocation_Clicked(object sender, RoutedEventArgs e)
    {
        try
        {
            var ipData = await IpApiClient.GetIpData();
            Global.SensitiveData.Lat = ipData.Lat;
            Global.SensitiveData.Lon = ipData.Lon;
        }
        catch (Exception exc)
        {
            Global.logger.Error(exc, "Failed getting geographic data");
        }
    }

    private void LocationReveal_Clicked(object sender, RoutedEventArgs e)
    {
        LocationPanel.Visibility = Visibility.Visible;
        LocationRevealButton.Visibility = Visibility.Hidden;
    }
}