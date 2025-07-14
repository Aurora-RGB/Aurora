using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Threading;
using AuroraRgb.Modules;
using AuroraRgb.Modules.Razer;
using RazerSdkReader;
using RazerSdkReader.Structures;
using Application = System.Windows.Application;
using MessageBox = System.Windows.MessageBox;
using Task = System.Threading.Tasks.Task;

namespace AuroraRgb.Settings.Controls.Wrappers;

public partial class Control_ChromaWrapper
{
    private readonly Task<ChromaSdkManager> _rzSdkManager;
    
    public Control_ChromaWrapper()
    {
        _rzSdkManager = RazerSdkModule.RzSdkManager;
        
        InitializeComponent();

        var rzVersion = RzHelper.GetSdkVersion();

        ChromaInstalledVersionLabel.Content = rzVersion.ToString();
        ChromaInstalledVersionLabel.Foreground = new SolidColorBrush(
            RzHelper.IsSdkVersionSupported(rzVersion) ? Colors.LightGreen : Colors.PaleVioletRed);
        ChromaSupportedVersionsLabel.Content = $"[{RzHelper.SupportedFromVersion}-{RzHelper.SupportedToVersion}]";

        if (rzVersion == new RzSdkVersion())
            ChromaAdvancedButton.Visibility = Visibility.Hidden;
    }

    private async void Control_ChromaWrapper_OnLoaded(object sender, RoutedEventArgs e)
    {
        await Load();
    }

    private async void Control_ChromaWrapper_OnUnloaded(object sender, RoutedEventArgs e)
    {
        await Unload();
    }

    private async Task Load()
    {
        await InitializeChromaEvents();
    }

    private async Task Unload()
    {
        var razerManager = (await _rzSdkManager).ChromaReader;
        if (razerManager != null)
        {
            razerManager.AppDataUpdated -= HandleChromaAppChange;
        }
    }

    private async Task InitializeChromaEvents()
    {
        var chromaSdkManager = await _rzSdkManager;
        chromaSdkManager.StateChanged += ChromaSdkManagerOnStateChanged;
        
        var chromaReader = chromaSdkManager.ChromaReader;
        UpdateChromaStatus(chromaReader);
    }

    private void ChromaSdkManagerOnStateChanged(object? sender, ChromaSdkStateChangedEventArgs e)
    {
        Dispatcher.BeginInvoke(() => UpdateChromaStatus(e.ChromaReader));
    }

    private void UpdateChromaStatus(ChromaReader? chromaReader)
    {
        if (chromaReader == null)
        {
            ChromaConnectionStatusLabel.Content = "Failure";
            ChromaConnectionStatusLabel.Foreground = new SolidColorBrush(Colors.PaleVioletRed);
            ChromaDisableDeviceControlButton.IsEnabled = false;
            ChromaInstallButton.IsEnabled = true;
            return;
        }

        ChromaConnectionStatusLabel.Content = "Success";
        ChromaConnectionStatusLabel.Foreground = new SolidColorBrush(Colors.LightGreen);
        ChromaDisableDeviceControlButton.IsEnabled = true;
        ChromaInstallButton.IsEnabled = false;

        var currentApp = RzHelper.CurrentAppExecutable;
        var currentAppId = RzHelper.CurrentAppId;
        ChromaCurrentApplicationLabel.Content = $"{currentApp ?? "None"} [{currentAppId}]";

        chromaReader.AppDataUpdated -= HandleChromaAppChange;
        chromaReader.AppDataUpdated += HandleChromaAppChange;
    }

    private void HandleChromaAppChange(object? s, in ChromaAppData appData)
    {
        uint currentAppId = 0;
        string? currentAppName = null;
        for (var i = 0; i < appData.AppCount; i++)
        {
            if (appData.CurrentAppId != appData.AppInfo[i].AppId) continue;

            currentAppId = appData.CurrentAppId;
            currentAppName = appData.AppInfo[i].AppName;
            break;
        }

        Dispatcher.BeginInvoke(DispatcherPriority.Loaded, 
            () => ChromaCurrentApplicationLabel.Content = $"{currentAppName} [{currentAppId}]");
    }

    private async void razer_wrapper_install_button_Click(object? sender, RoutedEventArgs e)
    {
        ChromaInstallButton.IsEnabled = false;
        ChromaAdvancedButton.IsEnabled = false;

        SetButtonContent("Uninstalling...");
        var uninstallSuccess = await ChromaInstallationUtils.UninstallAsync()
            .ContinueWith(async t =>
            {
                if (t.Exception != null)
                {
                    HandleExceptions(t.Exception);
                    return false;
                }

                if (await t != (int)RazerChromaInstallerExitCode.RestartRequired) return true;
                ShowMessageBox(
                    "The uninstaller requested system restart!\nPlease reboot your pc and re-run the installation.", "Restart required!");
                return false;
            })
            .ConfigureAwait(false);

        if (!await uninstallSuccess)
            return;

        SetButtonContent("Downloading...");
        var download = await ChromaInstallationUtils.DownloadAsync()
            .ContinueWith(t =>
            {
                if (t.Exception == null) return t;
                HandleExceptions(t.Exception);
                return Task.FromResult(null as string);
            })
            .ConfigureAwait(false);

        var downloadPath = await download;
        if (downloadPath == null)
            return;

        SetButtonContent("Installing...");
        await ChromaInstallationUtils.InstallAsync(downloadPath)
            .ContinueWith(async t =>
            {
                if (t.Exception != null)
                {
                    HandleExceptions(t.Exception);
                    return;
                }

                SetButtonContent("Disabling bloat...");
                ChromaInstallationUtils.DisableChromaBloat();
                SetButtonContent("Done!");

                if (await t == (int)RazerChromaInstallerExitCode.RestartRequired)
                    ShowMessageBox("The installer requested system restart!\nPlease reboot your pc.",
                        "Restart required!");
                else
                {
                    ShowMessageBox("Installation successful!\nRestart of Aurora may be needed.",
                        "Chroma SDK Installed!");
                }

                ChromaAdvancedButton.Visibility = Visibility.Visible;
            })
            .ConfigureAwait(false);

        void HandleExceptions(AggregateException ae)
        {
            ShowMessageBox(ae.ToString(), "Exception!", MessageBoxImage.Error);
            ae.Handle(ex => {
                Global.logger.Error(ex, "Chroma install error");
                return true;
            });
        }

        void SetButtonContent(string s) => Application.Current.Dispatcher.Invoke(() => ChromaInstallButton.Content = s);

        void ShowMessageBox(string message, string title, MessageBoxImage image = MessageBoxImage.Exclamation)
            => Application.Current.Dispatcher.Invoke(() => MessageBox.Show(message, title, MessageBoxButton.OK, image));
    }

    private async void razer_wrapper_disable_device_control_button_Click(object? sender, RoutedEventArgs e)
    {
        await ChromaInstallationUtils.DisableDeviceControlAsync();
    }

    private void ChromaAdvancedButton_OnClick(object sender, RoutedEventArgs e)
    {
        ChromaInstallButton.IsEnabled = false;
        ChromaAdvancedButton.IsEnabled = false;
        var chromaSettings = new Window_ChromaSettings();
        chromaSettings.Show();

        chromaSettings.Closed += (_, _) =>
        {
            ChromaInstallButton.IsEnabled = true;
            ChromaAdvancedButton.IsEnabled = true;
        };
    }
}