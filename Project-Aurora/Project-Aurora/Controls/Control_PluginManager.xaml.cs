using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using AuroraRgb.Settings;
using AuroraRgb.Utils;
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

    private void Control_PluginManager_OnLoaded(object sender, RoutedEventArgs e)
    {
        if (!IsVisible)
        {
            return;
        }

        UpdateInpOutStatus();
        UpdateWinRing0Status();
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

    private void UpdateWinRing0Status()
    {
        using var r = Registry.LocalMachine.OpenSubKey("SYSTEM\\CurrentControlSet\\Services\\WinRing0x64");
        if (r != null)
        {
            WinRing0Status.Foreground = Brushes.Coral;
            WinRing0Status.Text = "Exists";
            WinRing0DeleteButton.Visibility = Visibility.Visible;
        }
        else
        {
            WinRing0Status.Foreground = Brushes.Green;
            WinRing0Status.Text = "Not installed";
            WinRing0DeleteButton.Visibility = Visibility.Hidden;
        }
    }

    private void InpOutDeleteButton_OnClick(object sender, RoutedEventArgs e)
    {
        DeleteInpOut();
    }

    private void WinRing0DeleteButton_OnClick(object sender, RoutedEventArgs e)
    {
        DeleteWinRing0();
    }

    private void DeleteInpOut()
    {
        UnsecureDrivers.DeleteDriver(UnsecureDrivers.InpOutDriverName);
        UpdateInpOutStatus();
    }

    private void DeleteWinRing0()
    {
        UnsecureDrivers.DeleteDriver(UnsecureDrivers.WinRing0DriverName);
        UpdateWinRing0Status();
    }
}