using System.IO;
using System.Windows;
using AuroraRgb.Utils.Steam;

namespace AuroraRgb.Profiles.XCOM;

/// <summary>
/// Interaction logic for Control_XCOM.xaml
/// </summary>
public partial class Control_XCOM
{
    private readonly Application _profileManager;

    public Control_XCOM(Application profile)
    {
        InitializeComponent();

        _profileManager = profile;

        //Apply LightFX Wrapper, if needed.
        if (_profileManager.Settings?.InstallationCompleted ?? true) return;
        InstallWrapper();
        _profileManager.Settings.CompleteInstallation();
    }

    private void patch_button_Click(object? sender, RoutedEventArgs e)
    {
        if (InstallWrapper())
            MessageBox.Show("Aurora LightFX Wrapper installed successfully.");
        else
            MessageBox.Show("Aurora LightFX Wrapper could not be installed.\r\nGame is not installed.");
    }

    private void unpatch_button_Click(object? sender, RoutedEventArgs e)
    {
        if (UninstallWrapper())
            MessageBox.Show("Aurora LightFX Wrapper uninstalled successfully.");
        else
            MessageBox.Show("Aurora LightFX Wrapper could not be uninstalled.\r\nGame is not installed.");
    }

    private bool InstallWrapper(string installpath = "")
    {
        if (string.IsNullOrWhiteSpace(installpath))
            installpath = SteamUtils.GetGamePath(200510);

        if (string.IsNullOrWhiteSpace(installpath)) return false;
        var path = Path.Combine(installpath, "Binaries", "Win32", "LightFX.dll");

        if (!File.Exists(path))
            Directory.CreateDirectory(Path.GetDirectoryName(path));

        using var lightfxWrapper86 = new BinaryWriter(new FileStream(path, FileMode.Create));
        lightfxWrapper86.Write(Properties.Resources.Aurora_LightFXWrapper86);

        return true;

    }

    private bool UninstallWrapper()
    {
        var installPath = SteamUtils.GetGamePath(200510);
        if (string.IsNullOrWhiteSpace(installPath)) return false;
        var path = Path.Combine(installPath, "Binaries", "Win32", "LightFX.dll");

        if (File.Exists(path))
            File.Delete(path);

        return true;

    }
}