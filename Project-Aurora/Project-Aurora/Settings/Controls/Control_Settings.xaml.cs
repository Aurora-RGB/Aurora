using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Navigation;
using AuroraRgb.Controls;
using AuroraRgb.Devices;
using AuroraRgb.Devices.RGBNet.Config;
using AuroraRgb.Modules;
using AuroraRgb.Modules.GameStateListen;
using AuroraRgb.Modules.HardwareMonitor;
using AuroraRgb.Modules.Razer;
using Button = System.Windows.Controls.Button;
using MessageBox = System.Windows.MessageBox;

namespace AuroraRgb.Settings.Controls;

/// <summary>
/// Interaction logic for Control_Settings.xaml
/// </summary>
public partial class Control_Settings
{
    private readonly Task<AuroraHttpListener?> _httpListener;

    public Control_Settings(Task<ChromaSdkManager> rzSdkManager, Task<PluginManager> pluginManager, Task<AuroraHttpListener?> httpListener, Task<DeviceManager> deviceManager, Task<IpcListener?> ipcListener)
    {
        _httpListener = httpListener;
        InitializeComponent();

        var v = FileVersionInfo.GetVersionInfo(Assembly.GetExecutingAssembly().Location).FileVersion ?? "?";
        var o = FileVersionInfo.GetVersionInfo(Assembly.GetExecutingAssembly().Location).CompanyName ?? "Aurora-RGB";
        var r = FileVersionInfo.GetVersionInfo(Assembly.GetExecutingAssembly().Location).ProductName ?? "Aurora";

        lblVersion.Content = (v[0].ToString().Length > 0 ? "" : "beta ") + $"{v} {o}/{r}";
        LnkIssues.NavigateUri = new Uri($"https://github.com/{o}/{r}/issues/");
        LnkRepository.NavigateUri = new Uri($"https://github.com/{o}/{r}");
        LnkContributors.NavigateUri = new Uri($"https://github.com/{o}/{r}#contributors-");

        var devicesAndWrappers = new Control_SettingsDevicesAndWrappers(rzSdkManager, deviceManager);
        var controlDeviceManager = new Control_DeviceManager(deviceManager, ipcListener);
        var deviceMapping = new DeviceMapping(deviceManager, ipcListener);
        var plugins = new Control_PluginManager(pluginManager);
        
        DevicesAndWrappersTab.Content = devicesAndWrappers;
        DeviceManagerTab.Content = controlDeviceManager;
        RemapDevicesTab.Content = deviceMapping;
        PluginsTab.Content = plugins;
    }

    private void Hyperlink_RequestNavigate(object? sender, RequestNavigateEventArgs e)
    {
        Process.Start("explorer", e.Uri.AbsoluteUri);
        e.Handled = true;
    }

    private async void updates_check_Click(object? sender, RoutedEventArgs e)
    {
        if (!IsLoaded) return;
        await UpdateModule.CheckUpdate();
    }

    private void btnShowLogsFolder_Click(object? sender, RoutedEventArgs e)
    {
        if (sender is Button)
            Process.Start("explorer", Path.Combine(Global.LogsDirectory));
    }

    private void btnShowBitmapWindow_Click(object? sender, RoutedEventArgs e) => Window_BitmapView.Open();

    private void btnShowGSILog_Click(object? sender, RoutedEventArgs e) => Window_GSIHttpDebug.Open(_httpListener);

    private void btnDumpSensors_Click(object? sender, RoutedEventArgs e)
    {
        MessageBox.Show(HardwareMonitor.TryDump()
            ? "Successfully wrote sensor info to logs folder"
            : "Error dumping file. Consult log for details.");
    }
}