using System;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Media;
using System.Windows.Threading;
using AuroraRgb.Devices;
using AuroraRgb.Modules;
using AuroraRgb.Modules.Layouts;
using AuroraRgb.Modules.Logitech;
using AuroraRgb.Modules.Razer;
using RazerSdkReader;
using RazerSdkReader.Structures;
using Application = System.Windows.Application;
using MessageBox = System.Windows.MessageBox;

namespace AuroraRgb.Settings.Controls;

public partial class Control_SettingsDevicesAndWrappers
{
    private readonly Task<ChromaSdkManager> _rzSdkManager;
    private readonly Task<DeviceManager> _deviceManager;
    private readonly Task<KeyboardLayoutManager> _layoutManager;

    private const string StatusConflict = """✗""";
    private const string StatusCheck = """✔""";
    private const string StatusNoMatter = "‐";
    
    public Control_SettingsDevicesAndWrappers(Task<ChromaSdkManager> rzSdkManager, Task<DeviceManager> deviceManager, Task<KeyboardLayoutManager> layoutManager)
    {
        _rzSdkManager = rzSdkManager;
        _deviceManager = deviceManager;
        _layoutManager = layoutManager;

        InitializeComponent();

        var rzVersion = RzHelper.GetSdkVersion();

        ChromaInstalledVersionLabel.Content = rzVersion.ToString();
        ChromaInstalledVersionLabel.Foreground = new SolidColorBrush(
            RzHelper.IsSdkVersionSupported(rzVersion) ? Colors.LightGreen : Colors.PaleVioletRed);
        ChromaSupportedVersionsLabel.Content = $"[{RzHelper.SupportedFromVersion}-{RzHelper.SupportedToVersion}]";

        if (rzVersion == new RzSdkVersion())
            ChromaAdvancedButton.Visibility = Visibility.Hidden;
    }

    private async void Control_SettingsDevicesAndWrappers_OnUnloaded(object sender, RoutedEventArgs e)
    {
        await Unload();
    }

    private async Task Load()
    {
        await InitializeChromaEvents();
        await InitializeLightsyncEvents();
    }

    private async Task Unload()
    {
        var razerManager = (await _rzSdkManager).ChromaReader;
        if (razerManager != null)
        {
            razerManager.AppDataUpdated -= HandleChromaAppChange;
        }
        var logitechSdkListener = LogitechSdkModule.LogitechSdkListener;
        logitechSdkListener.ApplicationChanged -= LogitechSdkListenerOnApplicationChanged;
        logitechSdkListener.StateChanged -= LogitechSdkListenerOnStateChanged;
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

    private async Task InitializeLightsyncEvents()
    {
        var logitechSdkListener = LogitechSdkModule.LogitechSdkListener;
        logitechSdkListener.ApplicationChanged -= LogitechSdkListenerOnApplicationChanged;
        logitechSdkListener.ApplicationChanged += LogitechSdkListenerOnApplicationChanged;
        logitechSdkListener.StateChanged -= LogitechSdkListenerOnStateChanged;
        logitechSdkListener.StateChanged += LogitechSdkListenerOnStateChanged;
        await UpdateLightsyncState();
        UpdateLightsyncApp(logitechSdkListener.Application);
    }

    private async void LogitechSdkListenerOnStateChanged(object? sender, EventArgs e)
    {
        await UpdateLightsyncState();
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

    private void LogitechSdkListenerOnApplicationChanged(object? sender, string? e)
    {
        UpdateLightsyncApp(e);
    }

    private void UpdateLightsyncApp(string? appName)
    {
        Dispatcher.BeginInvoke(() =>
        {
            LightsyncCurrentApplicationLabel.Content = appName == null ? "-" : Path.GetFileName(appName);
        }, DispatcherPriority.Loaded);
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
                    break;
                case LightsyncSdkState.NotInstalled:
                    LightsyncConnectionStatusLabel.Content = "Not Installed";
                    LightsyncConnectionStatusLabel.Foreground = new SolidColorBrush(Colors.PaleVioletRed);
                    break;
                case LightsyncSdkState.Waiting:
                    LightsyncConnectionStatusLabel.Content = "Waiting for game";
                    LightsyncConnectionStatusLabel.Foreground = new SolidColorBrush(Colors.Chocolate);
                    break;
                case LightsyncSdkState.Connected:
                    LightsyncConnectionStatusLabel.Content = "Connected";
                    LightsyncConnectionStatusLabel.Foreground = new SolidColorBrush(Colors.Green);
                    break;
                case LightsyncSdkState.Disabled:
                    LightsyncConnectionStatusLabel.Content = "Disabled";
                    LightsyncConnectionStatusLabel.Foreground = new SolidColorBrush(Colors.PaleVioletRed);
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

    private void ResetDevices(object? sender, RoutedEventArgs e) => Task.Run(async () => await (await _deviceManager).ResetDevices());

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

    private void wrapper_install_lightfx_32_Click(object? sender, RoutedEventArgs e)
    {
        try
        {
            var dialog = new FolderBrowserDialog();
            var result = dialog.ShowDialog();

            if (result != DialogResult.OK) return;
            using (var lightfxWrapper86 = new BinaryWriter(new FileStream(Path.Combine(dialog.SelectedPath, "LightFX.dll"), FileMode.Create)))
            {
                lightfxWrapper86.Write(Properties.Resources.Aurora_LightFXWrapper86);
            }

            MessageBox.Show("Aurora Wrapper Patch for LightFX (32 bit) applied to\r\n" + dialog.SelectedPath);
        }
        catch (Exception exc)
        {
            Global.logger.Error(exc, "Exception during LightFX (32 bit) Wrapper install. Exception: ");
            MessageBox.Show("Aurora Wrapper Patch for LightFX (32 bit) could not be applied.\r\nException: " + exc.Message);
        }
    }

    private void wrapper_install_lightfx_64_Click(object? sender, RoutedEventArgs e)
    {
        try
        {
            var dialog = new FolderBrowserDialog();
            var result = dialog.ShowDialog();

            if (result != DialogResult.OK) return;
            using (var lightfxWrapper64 = new BinaryWriter(new FileStream(Path.Combine(dialog.SelectedPath, "LightFX.dll"), FileMode.Create)))
            {
                lightfxWrapper64.Write(Properties.Resources.Aurora_LightFXWrapper64);
            }

            MessageBox.Show("Aurora Wrapper Patch for LightFX (64 bit) applied to\r\n" + dialog.SelectedPath);
        }
        catch (Exception exc)
        {
            Global.logger.Error(exc, "Exception during LightFX (64 bit) Wrapper install");
            MessageBox.Show("Aurora Wrapper Patch for LightFX (64 bit) could not be applied.\r\nException: " + exc.Message);
        }
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

    private async void LayoutsRefreshButton_OnClick(object sender, RoutedEventArgs e)
    {
        var keyboardLayoutManager = await _layoutManager;
        await keyboardLayoutManager.LoadBrandDefault();
    }
}