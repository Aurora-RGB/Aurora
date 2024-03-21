using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using AuroraRgb.Utils;
using AuroraRgb.Utils.IpApi;
using Xceed.Wpf.Toolkit;

namespace AuroraRgb.Settings.Controls;

public partial class Control_SettingsGeneral
{
    /// <summary>The excluded program the user has selected in the excluded list.</summary>
    private string SelectedExcludedProgram { get; set; } = string.Empty;
    
    public Control_SettingsGeneral()
    {
        InitializeComponent();

        TransparencyCheckbox.IsEnabled = TransparencyComponent.UseMica;
    }

    private void Control_SettingsGeneral_OnLoaded(object sender, RoutedEventArgs e)
    {
        DataContext = Global.Configuration;
        RunAtWinStartup.IsChecked = AutoStartUtils.GetAutostartTask(out var delayAmount);
        startDelayAmount.Value = delayAmount;
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
        if (!string.IsNullOrEmpty(SelectedExcludedProgram))
            Global.Configuration.ExcludedPrograms.Remove(SelectedExcludedProgram);
    }

    private void RunAtWinStartup_Checked(object? sender, RoutedEventArgs e)
    {
        if (!IsLoaded || sender is not CheckBox checkBox) return;
        AutoStartUtils.SetEnabled(checkBox.IsChecked.GetValueOrDefault());
    }

    private void HighPriorityCheckbox_Checked(object? sender, RoutedEventArgs e)
    {
        if (!IsLoaded) return;
        Process.GetCurrentProcess().PriorityClass = Global.Configuration.HighPriority ? ProcessPriorityClass.High : ProcessPriorityClass.Normal;
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
            Global.Configuration.Lat = ipData.Lat;
            Global.Configuration.Lon = ipData.Lon;
        }
        catch (Exception exc)
        {
            Global.logger.Error(exc, "Failed getting geographic data");
        }
    }
}