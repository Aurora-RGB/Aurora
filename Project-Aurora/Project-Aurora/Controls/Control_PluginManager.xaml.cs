using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using AuroraRgb.Settings;
using Microsoft.Win32;

namespace AuroraRgb.Controls;

/// <summary>
/// Interaction logic for Control_DeviceManager.xaml
/// </summary>
public partial class Control_PluginManager
{
    private readonly Task<PluginManager> _host;

    public Control_PluginManager(Task<PluginManager> host)
    {
        _host = host;

        InitializeComponent();

        DataContext = host;
    }

    private async void chkEnabled_Checked(object? sender, RoutedEventArgs e)
    {
        if (sender == null)
            return;
        var chk = (CheckBox)sender;
        var plugin = (KeyValuePair<string, IPlugin>)chk.DataContext;
        (await _host).SetPluginEnabled(plugin.Key, (bool)chk.IsChecked);
    }

    private void AmdMonitorToggleButton_OnChecked(object sender, RoutedEventArgs e)
    {
        var messageBoxResult = MessageBox.Show(
            "Removing the drivers by this module can be annoying. Do you still want to continue?",
            "Confirm action", MessageBoxButton.YesNo,
            MessageBoxImage.Warning);
        if (messageBoxResult == MessageBoxResult.Yes) return;
        e.Handled = true;
        Global.Configuration.EnableAmdCpuMonitor = false;
    }

    private void Control_PluginManager_OnLoaded(object sender, RoutedEventArgs e)
    {
        if (!IsVisible)
        {
            return;
        }
        AmdMonitorToggle.Checked -= AmdMonitorToggleButton_OnChecked;
        AmdMonitorToggle.Checked += AmdMonitorToggleButton_OnChecked;

        UpdateInpOutStatus();
    }

    private void UpdateInpOutStatus()
    {
        using var r = Registry.LocalMachine.OpenSubKey("SYSTEM\\CurrentControlSet\\Services\\inpoutx64");
        if (r != null)
        {
            InpOut64Status.Foreground = Brushes.Red;
            InpOut64Status.Text = "Exists";
            InpOutDeleteButton.Visibility = Visibility.Visible;
        }
        else
        {
            InpOut64Status.Foreground = Brushes.Green;
            InpOut64Status.Text = "Not installed";
            InpOutDeleteButton.Visibility = Visibility.Hidden;
        }
    }

    private void Control_PluginManager_OnUnloaded(object sender, RoutedEventArgs e)
    {
        AmdMonitorToggle.Checked -= AmdMonitorToggleButton_OnChecked;
    }

    private void InpOutDeleteButton_OnClick(object sender, RoutedEventArgs e)
    {
        DeleteInpOut();
    }

    private void DeleteInpOut()
    {
        const string inpOutKey = @"SYSTEM\CurrentControlSet\Services\inpoutx64";

        try
        {
            using var r = Registry.LocalMachine.OpenSubKey(inpOutKey);
            if (r == null)
            {
                MessageBox.Show("Driver isn't installed", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            var imagePath = r.GetValue("ImagePath");
            var inpOutSysPath = Environment.GetFolderPath(Environment.SpecialFolder.Windows) + Path.PathSeparator + imagePath;

            Registry.LocalMachine.DeleteSubKeyTree(inpOutKey);

            try
            {
                if (File.Exists(inpOutSysPath))
                {
                    File.Delete(inpOutSysPath);
                }

                MessageBox.Show("Deleted driver. Your system needs restart to fully take effect");
            }
            catch
            {
                Global.logger.Error("inpoutx64.sys could not be deleted");
                MessageBox.Show("inpoutx64.sys could not be deleted. Restart your system and delete this file manually:\n" + inpOutSysPath);
            }
        }
        catch(Exception e)
        {
            Global.logger.Error(e, "Could not delete input64 registry keys");
        }
        UpdateInpOutStatus();
    }
}