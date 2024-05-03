using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Media;
using System.Windows.Threading;
using AuroraRgb.Devices;
using AuroraRgb.Modules;
using AuroraRgb.Modules.Logitech;
using AuroraRgb.Modules.Razer;
using Microsoft.Win32;
using RazerSdkReader;
using RazerSdkReader.Structures;
using Application = System.Windows.Application;
using MessageBox = System.Windows.MessageBox;

namespace AuroraRgb.Settings.Controls;

public partial class Control_SettingsDevicesAndWrappers
{
    private readonly Task<ChromaSdkManager> _rzSdkManager;
    private readonly Task<DeviceManager> _deviceManager;
    
    public Control_SettingsDevicesAndWrappers(Task<ChromaSdkManager> rzSdkManager,
        Task<DeviceManager> deviceManager)
    {
        _rzSdkManager = rzSdkManager;
        _deviceManager = deviceManager;

        InitializeComponent();

        var rzVersion = RzHelper.GetSdkVersion();

        ChromaInstalledVersionLabel.Content = rzVersion.ToString();
        ChromaInstalledVersionLabel.Foreground = new SolidColorBrush(
            RzHelper.IsSdkVersionSupported(rzVersion) ? Colors.LightGreen : Colors.PaleVioletRed);
        ChromaSupportedVersionsLabel.Content = $"[{RzHelper.SupportedFromVersion}-{RzHelper.SupportedToVersion}]";

        if (rzVersion == new RzSdkVersion())
            ChromaUninstallButton.Visibility = Visibility.Hidden;
    }

    private async void Control_SettingsDevicesAndWrappers_OnUnloaded(object sender, RoutedEventArgs e)
    {
        await Unload();
    }

    private async Task Load()
    {
        await InitializeChromaEvents();
        InitializeLightsyncEvents();
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
            return;
        }

        ChromaConnectionStatusLabel.Content = "Success";
        ChromaConnectionStatusLabel.Foreground = new SolidColorBrush(Colors.LightGreen);

        var currentApp = RzHelper.CurrentAppExecutable;
        var currentAppId = RzHelper.CurrentAppId;
        ChromaCurrentApplicationLabel.Content = $"{currentApp ?? "None"} [{currentAppId}]";

        chromaReader.AppDataUpdated -= HandleChromaAppChange;
        chromaReader.AppDataUpdated += HandleChromaAppChange;
    }

    private void InitializeLightsyncEvents()
    {
        var logitechSdkListener = LogitechSdkModule.LogitechSdkListener;
        logitechSdkListener.ApplicationChanged -= LogitechSdkListenerOnApplicationChanged;
        logitechSdkListener.ApplicationChanged += LogitechSdkListenerOnApplicationChanged;
        logitechSdkListener.StateChanged -= LogitechSdkListenerOnStateChanged;
        logitechSdkListener.StateChanged += LogitechSdkListenerOnStateChanged;
        UpdateLightsyncState();
        UpdateLightsyncApp(logitechSdkListener.Application);
    }

    private void LogitechSdkListenerOnStateChanged(object? sender, EventArgs e)
    {
        UpdateLightsyncState();
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

    private void UpdateLightsyncState()
    {
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
        }, DispatcherPriority.Loaded);
    }

    private void ResetDevices(object? sender, RoutedEventArgs e) => Task.Run(async () => await (await _deviceManager).ResetDevices());

    private async void razer_wrapper_install_button_Click(object? sender, RoutedEventArgs e)
    {
        ChromaInstallButton.IsEnabled = false;
        ChromaUninstallButton.IsEnabled = false;

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
                    "The uninstaller requested system restart!\nPlease reboot your pc and re-run the installation.",
                    "Restart required!");
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
                    HandleExceptions(t.Exception);
                else if (await t == (int)RazerChromaInstallerExitCode.RestartRequired)
                    ShowMessageBox("The installer requested system restart!\nPlease reboot your pc.",
                        "Restart required!");
                else
                {
                    SetButtonContent("Disabling bloat...");
                    ChromaInstallationUtils.DisableChromaBloat();
                    SetButtonContent("Done!");
                    ShowMessageBox("Installation successful!\nPlease restart aurora for changes to take effect.",
                        "Restart required!");
                }
            })
            .ConfigureAwait(false);
        return;

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

    private void razer_wrapper_uninstall_button_Click(object? sender, RoutedEventArgs e)
    {
        void HandleExceptions(AggregateException ae)
        {
            ShowMessageBox(ae.ToString(), "Exception!", MessageBoxImage.Error);
            ae.Handle(ex => {
                Global.logger.Error(ex, "Razer wrapper uninstall error");
                return true;
            });
        }

        void SetButtonContent(string s)
            => Application.Current.Dispatcher.Invoke(() => ChromaUninstallButton.Content = s);

        void ShowMessageBox(string message, string title, MessageBoxImage image = MessageBoxImage.Exclamation)
            => Application.Current.Dispatcher.Invoke(() => MessageBox.Show(message, title, MessageBoxButton.OK, image));

        ChromaInstallButton.IsEnabled = false;
        ChromaUninstallButton.IsEnabled = false;

        Task.Run(async () =>
        {
            SetButtonContent("Uninstalling");
            await ChromaInstallationUtils.UninstallAsync()
                .ContinueWith(async t =>
                {
                    if (t.Exception != null)
                        HandleExceptions(t.Exception);
                    else if (await t == (int)RazerChromaInstallerExitCode.RestartRequired)
                        ShowMessageBox("The uninstaller requested system restart!\nPlease reboot your pc.", "Restart required!");
                    else if (await t == (int)RazerChromaInstallerExitCode.InvalidState)
                        ShowMessageBox("There is nothing to install!", "Invalid State!");
                    else
                    {
                        SetButtonContent("Done!");
                        ShowMessageBox("Uninstallation successful!\nPlease restart aurora for changes to take effect.", "Restart required!");
                    }
                })
                .ConfigureAwait(false);
        });
    }

    private async void razer_wrapper_disable_device_control_button_Click(object? sender, RoutedEventArgs e)
    {
        await ChromaInstallationUtils.DisableDeviceControlAsync();
    }

    private void Lightsync_install_button_Click(object? sender, RoutedEventArgs e)
    {
        const string wrapperFolder = "C:\\ProgramData\\Aurora";
        const string dllName = "LogitechLed.dll";
        
        using var httpClient = new HttpClient();

        try
        {
            InstallLightsync64(wrapperFolder, dllName, httpClient);
            InstallLightsync86(wrapperFolder, dllName, httpClient);

            LightsyncConnectionStatusLabel.Content = "Waiting for game";
            MessageBox.Show("Logitech wrapper installed!");
        }
        catch (Exception exc)
        {
            Global.logger.Error(exc, "Error installing Lightsync wrapper!");
            MessageBox.Show(exc.Message, "Error installing Lightsync wrapper!");
        }
    }

    private const string ArtemisRepo =
        "https://raw.githubusercontent.com/Artemis-RGB/Artemis.Plugins.Wrappers/04d1f6acf3a93b0c883118f174b18ced8e264a84";
    private static void InstallLightsync64(string wrapperFolder, string dllName, HttpClient httpClient)
    {
        const string logiX64 = ArtemisRepo + "/src/Logitech/Artemis.Plugins.Wrappers.Logitech/x64/LogitechLed.dll";
        var x64DllFolder = Path.Combine(wrapperFolder, "x64");
        var x64DllPath = Path.Combine(x64DllFolder, dllName);

        var x64Response = httpClient.GetAsync(new Uri(logiX64));
        Directory.CreateDirectory(x64DllFolder);
        using var fs = new FileStream(x64DllPath, FileMode.Create);
        x64Response.Result.Content.CopyToAsync(fs).Wait();

        using var key =Registry.LocalMachine.CreateSubKey(
                @"SOFTWARE\Classes\CLSID\{a6519e67-7632-4375-afdf-caa889744403}\ServerBinary", true);
        key.SetValue(null, x64DllPath);
    }

    private static void InstallLightsync86(string wrapperFolder, string dllName, HttpClient httpClient)
    {
        const string logiX86 = ArtemisRepo + "/src/Logitech/Artemis.Plugins.Wrappers.Logitech/x86/LogitechLed.dll";
        var x86DllFolder = Path.Combine(wrapperFolder, "x86");

        var x86Response = httpClient.GetAsync(new Uri(logiX86));
        Directory.CreateDirectory(x86DllFolder);
        var x86DllPath = Path.Combine(x86DllFolder, dllName);
        using var fs = new FileStream(x86DllPath, FileMode.Create);
        x86Response.Result.Content.CopyToAsync(fs).Wait();

        using var key = Registry.LocalMachine.CreateSubKey(
            @"SOFTWARE\Classes\WOW6432Node\CLSID\{a6519e67-7632-4375-afdf-caa889744403}\ServerBinary", true);
        key.SetValue(null, x86DllPath);
    }

    private void wrapper_install_lightfx_32_Click(object? sender, RoutedEventArgs e)
    {
        try
        {
            var dialog = new FolderBrowserDialog();
            DialogResult result = dialog.ShowDialog();

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
            DialogResult result = dialog.ShowDialog();

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
}