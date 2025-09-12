using System;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Threading;
using AuroraRgb.Modules;
using AuroraRgb.Modules.Logitech;

namespace AuroraRgb.Settings.Controls.Wrappers;

public partial class Control_LightsyncWrapper
{
    private const string StatusConflict = """✗""";
    private const string StatusCheck = """✔""";
    private const string StatusNoMatter = "‐";

    private readonly LogitechSdkListener _logitechSdkListener = LogitechSdkModule.LogitechSdkListener;

    public Control_LightsyncWrapper()
    {
        InitializeComponent();
    }

    private async void Control_SettingsDevicesAndWrappers_OnIsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
    {
        if (!IsVisible)
        {
            await Unload();
        }
        else
        {
            await Load();
        }
    }

    private async Task Load()
    {
        await InitializeLightsyncEvents();
    }

    private async Task Unload()
    {
        var logitechSdkListener = LogitechSdkModule.LogitechSdkListener;
        logitechSdkListener.ApplicationChanged -= LogitechSdkListenerOnApplicationChanged;
        logitechSdkListener.StateChanged -= LogitechSdkListenerOnStateChanged;
    }

    private async Task InitializeLightsyncEvents()
    {
        _logitechSdkListener.ApplicationChanged -= LogitechSdkListenerOnApplicationChanged;
        _logitechSdkListener.ApplicationChanged += LogitechSdkListenerOnApplicationChanged;
        _logitechSdkListener.StateChanged -= LogitechSdkListenerOnStateChanged;
        _logitechSdkListener.StateChanged += LogitechSdkListenerOnStateChanged;
        await UpdateLightsyncState();
        UpdateLightsyncApp(_logitechSdkListener.Application);
    }

    private async void LogitechSdkListenerOnStateChanged(object? sender, EventArgs e)
    {
        await UpdateLightsyncState();
    }

    private void LogitechSdkListenerOnApplicationChanged(object? sender, string? e)
    {
        UpdateLightsyncApp(e);
    }

    private void UpdateLightsyncApp(string? appName)
    {
        Dispatcher.BeginInvoke(() => { LightsyncCurrentApplicationLabel.Content = appName == null ? "-" : Path.GetFileName(appName); },
            DispatcherPriority.Loaded);
    }

    private async Task UpdateLightsyncState()
    {
        var isLgsInstalled = LgsInstallationUtils.IsLgsInstalled();
        var lgsAutorunEnabled = LgsInstallationUtils.LgsAutorunEnabled();
        var dllInstalled = LgsInstallationUtils.DllInstalled();

        var runningProcessMonitor = await ProcessesModule.RunningProcessMonitor;
        var lgsRunning = runningProcessMonitor.IsProcessRunning(LgsInstallationUtils.LgsExe);

        Dispatcher.BeginInvoke(() =>
        {
            var logitechSdkListener = LogitechSdkModule.LogitechSdkListener;
            switch (logitechSdkListener.State)
            {
                case LightsyncSdkState.Conflicted:
                    LightsyncConnectionStatusLabel.Content = "Conflicted";
                    LightsyncConnectionStatusLabel.Foreground = new SolidColorBrush(Colors.Crimson);
                    LightsyncCurrentApplicationLabel.Content = "-";
                    break;
                case LightsyncSdkState.NotInstalled:
                    LightsyncConnectionStatusLabel.Content = "Not Installed";
                    LightsyncConnectionStatusLabel.Foreground = new SolidColorBrush(Colors.PaleVioletRed);
                    LightsyncCurrentApplicationLabel.Content = "-";
                    break;
                case LightsyncSdkState.Waiting:
                    LightsyncConnectionStatusLabel.Content = "Waiting for game";
                    LightsyncConnectionStatusLabel.Foreground = new SolidColorBrush(Colors.Chocolate);
                    LightsyncCurrentApplicationLabel.Content = "-";
                    break;
                case LightsyncSdkState.Connected:
                    LightsyncConnectionStatusLabel.Content = "Connected";
                    LightsyncConnectionStatusLabel.Foreground = new SolidColorBrush(Colors.Green);
                    LightsyncCurrentApplicationLabel.Content = logitechSdkListener.Application;
                    break;
                case LightsyncSdkState.Disabled:
                    LightsyncConnectionStatusLabel.Content = "Disabled";
                    LightsyncConnectionStatusLabel.Foreground = new SolidColorBrush(Colors.PaleVioletRed);
                    LightsyncCurrentApplicationLabel.Content = "-";
                    break;
                default:
                    Global.logger.Error("LogitechSdkListener.State: {0} Unexpected Enum value", logitechSdkListener.State);
                    break;
            }

            SetAutostartLabel(isLgsInstalled, lgsAutorunEnabled);
            SetProcessRunningLabel(lgsRunning);
            SetDllInstalledLabel(dllInstalled);
        }, DispatcherPriority.Loaded);
    }

    private void SetAutostartLabel(bool isLgsInstalled, bool lgsAutorunEnabled)
    {
        if (isLgsInstalled)
        {
            if (lgsAutorunEnabled)
            {
                LgsAutostartStatus.Text = StatusConflict;
                LgsAutostartStatus.Foreground = Brushes.Red;
            }
            else
            {
                LgsAutostartStatus.Text = StatusCheck;
                LgsAutostartStatus.Foreground = Brushes.Green;
            }
        }
        else
        {
            LgsAutostartStatus.Text = StatusNoMatter;
            LgsAutostartStatus.Foreground = Brushes.LightGray;
        }
    }

    private void SetProcessRunningLabel(bool lgsRunning)
    {
        if (lgsRunning)
        {
            LgsRunningStatus.Text = StatusConflict;
            LgsRunningStatus.Foreground = Brushes.Red;
        }
        else
        {
            LgsRunningStatus.Text = StatusCheck;
            LgsRunningStatus.Foreground = Brushes.Green;
        }
    }

    private void SetDllInstalledLabel(bool dllInstalled)
    {
        if (dllInstalled)
        {
            LightsyncDllStatus.Text = StatusCheck;
            LightsyncDllStatus.Foreground = Brushes.Green;
        }
        else
        {
            LightsyncDllStatus.Text = StatusConflict;
            LightsyncDllStatus.Foreground = Brushes.Red;
        }
    }
}