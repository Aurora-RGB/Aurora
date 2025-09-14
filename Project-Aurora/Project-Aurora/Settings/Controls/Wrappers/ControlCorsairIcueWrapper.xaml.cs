using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Threading;
using AuroraRgb.Modules;
using AuroraRgb.Modules.Icue;

namespace AuroraRgb.Settings.Controls.Wrappers;

public partial class ControlCorsairIcueWrapper
{
    private const string StatusConflict = """✗""";
    private const string StatusCheck = """✔""";
    private const string StatusNoMatter = "‐";
    
    public ControlCorsairIcueWrapper()
    {
        InitializeComponent();
    }

    private async void ControlCorsairIcueWrapper_OnLoaded(object sender, RoutedEventArgs e)
    {
        IcueModule.AuroraIcueServer.StatusChanged += AuroraIcueServerOnStatusChanged;
        IcueModule.AuroraIcueServer.Sdk.GameChanged += SdkOnGameChanged;
        await UpdateIcueState();
        UpdateConnectedGame();
    }

    private void ControlCorsairIcueWrapper_OnUnloaded(object sender, RoutedEventArgs e)
    {
        IcueModule.AuroraIcueServer.StatusChanged -= AuroraIcueServerOnStatusChanged;
    }

    private void AuroraIcueServerOnStatusChanged(object? sender, EventArgs e)
    {
        Dispatcher.BeginInvoke(async () =>
        {
            await UpdateIcueState();
        });
    }

    private void SdkOnGameChanged(object? sender, EventArgs e)
    {
        Dispatcher.BeginInvoke(UpdateConnectedGame);
    }

    private async Task UpdateIcueState()
    {
        var isInstalled = IcueInstallationUtils.IsIcueInstalled();
        var lgsAutorunEnabled = IcueInstallationUtils.IsIcueAutorunEnabled();

        var runningProcessMonitor = await ProcessesModule.RunningProcessMonitor;
        var icueRunning = runningProcessMonitor.IsProcessRunning(IcueInstallationUtils.IcueExe);

        _ = Dispatcher.BeginInvoke(() =>
        {
            var icueSdk = IcueModule.AuroraIcueServer;
            switch (icueSdk.ServerStatus)
            {
                case IcueServerStatus.Conflicted:
                    StatusTextBlock.Foreground = new SolidColorBrush(Colors.Crimson);
                    StatusTextBlock.Text = "Conflicted";
                    break;
                case IcueServerStatus.Waiting:
                    StatusTextBlock.Foreground = new SolidColorBrush(Colors.Chocolate);
                    StatusTextBlock.Text = "Waiting for Game";
                    break;
                case IcueServerStatus.Disabled:
                    StatusTextBlock.Foreground = new SolidColorBrush(Colors.PaleVioletRed);
                    StatusTextBlock.Text = "Disabled";
                    break;
                case IcueServerStatus.PipesInUse:
                    StatusTextBlock.Foreground = new SolidColorBrush(Colors.OrangeRed);
                    StatusTextBlock.Text = "Pipes in Use";
                    break;
                default:
                    Global.logger.Error("LogitechSdkListener.State: {0} Unexpected Enum value", icueSdk.ServerStatus);
                    break;
            }

            SetAutostartLabel(isInstalled, lgsAutorunEnabled);
            SetProcessRunningLabel(icueRunning);
        }, DispatcherPriority.Loaded);
    }
    
    private void UpdateConnectedGame()
    {
        var sdkGamePid = (int)IcueModule.AuroraIcueServer.Sdk.GamePid;
        var gameName = IcueModule.AuroraIcueServer.Sdk.GameProcess;
        
        IcueCurrentApplicationLabel.Content = $"{gameName} ({sdkGamePid})";
    }

    private void SetAutostartLabel(bool isInstalled, bool isAutorunEnabled)
    {
        if (isInstalled)
        {
            if (isAutorunEnabled)
            {
                IcueAutostartStatus.Text = StatusConflict;
                IcueAutostartStatus.Foreground = Brushes.Red;
            }
            else
            {
                IcueAutostartStatus.Text = StatusCheck;
                IcueAutostartStatus.Foreground = Brushes.Green;
            }
        }
        else
        {
            IcueAutostartStatus.Text = StatusNoMatter;
            IcueAutostartStatus.Foreground = Brushes.LightGray;
        }
    }

    private void SetProcessRunningLabel(bool isRunning)
    {
        if (isRunning)
        {
            IcueRunningStatus.Text = StatusConflict;
            IcueRunningStatus.Foreground = Brushes.Red;
        }
        else
        {
            IcueRunningStatus.Text = StatusCheck;
            IcueRunningStatus.Foreground = Brushes.Green;
        }
    }
}