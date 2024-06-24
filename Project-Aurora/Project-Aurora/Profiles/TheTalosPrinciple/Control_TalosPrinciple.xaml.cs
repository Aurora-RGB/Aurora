using System.IO;
using System.Windows;
using AuroraRgb.Utils.Steam;

namespace AuroraRgb.Profiles.TheTalosPrinciple;

/// <summary>
/// Interaction logic for Control_TalosPrinciple.xaml
/// </summary>
public partial class Control_TalosPrinciple
{
    private readonly Application _profileManager;

    public Control_TalosPrinciple(Application profile)
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
            installpath = SteamUtils.GetGamePath(257510);


        if (string.IsNullOrWhiteSpace(installpath)) return false;
        //86
        var path = Path.Combine(installpath, "Bin", "LightFX.dll");

        if (!File.Exists(path))
            Directory.CreateDirectory(Path.GetDirectoryName(path));

        using (var lightfx_wrapper_86 = new BinaryWriter(new FileStream(path, FileMode.Create)))
        {
            lightfx_wrapper_86.Write(Properties.Resources.Aurora_LightFXWrapper86);
        }

        //64
        var path64 = Path.Combine(installpath, "Bin", "x64", "LightFX.dll");

        if (!File.Exists(path64))
            Directory.CreateDirectory(Path.GetDirectoryName(path64));

        using (var lightfxWrapper64 = new BinaryWriter(new FileStream(path64, FileMode.Create)))
        {
            lightfxWrapper64.Write(Properties.Resources.Aurora_LightFXWrapper64);
        }

        return true;

    }

    private bool UninstallWrapper()
    {
        var installPath = SteamUtils.GetGamePath(257510);
        if (string.IsNullOrWhiteSpace(installPath)) return false;
        //86
        var path = Path.Combine(installPath, "Bin", "LightFX.dll");

        if (File.Exists(path))
            File.Delete(path);

        //64
        var path64 = Path.Combine(installPath, "Bin", "x64", "LightFX.dll");

        if (File.Exists(path64))
            File.Delete(path64);

        return true;
    }
}