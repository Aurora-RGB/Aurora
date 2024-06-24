using System.IO;
using System.Windows;
using AuroraRgb.Utils.Steam;

namespace AuroraRgb.Profiles.Blacklight;

/// <summary>
/// Interaction logic for Control_BLight.xaml
/// </summary>
public partial class Control_BLight
{
    private Application _profileManager;

    public Control_BLight(Application profile)
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

    private void UserControl_Loaded(object? sender, RoutedEventArgs e)
    {
    }

    private void UserControl_Unloaded(object? sender, RoutedEventArgs e)
    {
    }

    private bool InstallWrapper(string installPath = "")
    {
        if (string.IsNullOrWhiteSpace(installPath))
            installPath = SteamUtils.GetGamePath(209870);

        if (string.IsNullOrWhiteSpace(installPath)) return false;
        //86
        var path = Path.Combine(installPath, "Binaries", "Win32", "LightFX.dll");

        if (!File.Exists(path))
            Directory.CreateDirectory(Path.GetDirectoryName(path));

        using var lightfx_wrapper_86 = new BinaryWriter(new FileStream(path, FileMode.Create));
        lightfx_wrapper_86.Write(Properties.Resources.Aurora_LightFXWrapper86);

        return true;

    }

    private bool UninstallWrapper(string installpath = "")
    {
        if (string.IsNullOrWhiteSpace(installpath))
            installpath = SteamUtils.GetGamePath(209870);

        if (string.IsNullOrWhiteSpace(installpath)) return false;
        //86
        var path = Path.Combine(installpath, "Binaries", "Win32", "LightFX.dll");

        if (File.Exists(path))
            File.Delete(path);

        return true;

    }
}