using System.IO;
using System.Windows;
using AuroraRgb.Settings;
using AuroraRgb.Utils.Steam;

namespace AuroraRgb.Profiles.DiscoDodgeball;

/// <summary>
/// Interaction logic for Control_DiscoDodgeball.xaml
/// </summary>
public partial class Control_DiscoDodgeball
{
    private Application profile_manager;

    public Control_DiscoDodgeball(Application profile)
    {
        InitializeComponent();

        profile_manager = profile;

        //Apply LightFX Wrapper, if needed.
        if ((profile_manager.Settings as FirstTimeApplicationSettings).IsFirstTimeInstalled) return;
        InstallWrapper();
        (profile_manager.Settings as FirstTimeApplicationSettings).IsFirstTimeInstalled = true;
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

    private bool InstallWrapper(string installpath = "")
    {
        if (string.IsNullOrWhiteSpace(installpath))
            installpath = SteamUtils.GetGamePath(270450);


        if (string.IsNullOrWhiteSpace(installpath)) return false;
        var path = Path.Combine(installpath, "LightFX.dll");

        if (!File.Exists(path))
            Directory.CreateDirectory(Path.GetDirectoryName(path));

        using var lightfxWrapper86 = new BinaryWriter(new FileStream(path, FileMode.Create));
        lightfxWrapper86.Write(Properties.Resources.Aurora_LightFXWrapper86);

        return true;
    }

    private bool UninstallWrapper()
    {
        var installPath = SteamUtils.GetGamePath(270450);
        if (string.IsNullOrWhiteSpace(installPath)) return false;
        var path = Path.Combine(installPath, "LightFX.dll");

        if (File.Exists(path))
            File.Delete(path);

        return true;

    }
}