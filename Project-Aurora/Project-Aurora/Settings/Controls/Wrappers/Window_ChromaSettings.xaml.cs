using System;
using System.Threading.Tasks;
using System.Windows;
using AuroraRgb.Modules.Razer;
using AuroraRgb.Utils;

namespace AuroraRgb.Settings.Controls.Wrappers;

public partial class Window_ChromaSettings : IDisposable
{
    private readonly TransparencyComponent _transparencyComponent;
    public Window_ChromaSettings()
    {
        InitializeComponent();

        _transparencyComponent = new TransparencyComponent(this, null);
    }

    private void razer_wrapper_uninstall_button_Click(object? sender, RoutedEventArgs e)
    {
        RestoreDeviceControlButton.IsEnabled = false;
        ChromaDisableBloatButton.IsEnabled = false;
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
                        Application.Current.Dispatcher.Invoke(() =>
                        {
                            RestoreDeviceControlButton.IsEnabled = true;
                            ChromaDisableBloatButton.IsEnabled = true;
                        });
                        ShowMessageBox("Uninstallation successful!\nPlease restart aurora for changes to take effect.", "Restart required!");
                    }
                })
                .ConfigureAwait(false);
        });

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
    }

    public void Dispose()
    {
        _transparencyComponent.Dispose();
    }

    private void RestoreDeviceControlButton_OnClick(object sender, RoutedEventArgs e)
    {
        ChromaInstallationUtils.RestoreDeviceControl();
    }

    private void ChromaDisableBloatButton_OnClick(object sender, RoutedEventArgs e)
    {
        ChromaInstallationUtils.DisableChromaBloat();
    }
}