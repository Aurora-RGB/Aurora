using System.IO;
using System.Windows;
using AuroraRgb.Settings;
using AuroraRgb.Utils.Steam;

namespace AuroraRgb.Profiles.KillingFloor2;

/// <summary>
/// Interaction logic for Control_KillingFloor2.xaml
/// </summary>
public partial class Control_KillingFloor2
{
    private readonly Application _profileManager;

    public Control_KillingFloor2(Application profile)
    {
        InitializeComponent();

        _profileManager = profile;

        //Apply LightFX Wrapper, if needed.
        if ((_profileManager.Settings as FirstTimeApplicationSettings).IsFirstTimeInstalled) return;
        InstallWrapper();
        (_profileManager.Settings as FirstTimeApplicationSettings).IsFirstTimeInstalled = true;
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
            installpath = SteamUtils.GetGamePath(232090);


        if (string.IsNullOrWhiteSpace(installpath)) return false;
        var path = Path.Combine(installpath, "Binaries", "Win64", "LightFX.dll");

        if (!File.Exists(path))
            Directory.CreateDirectory(Path.GetDirectoryName(path));

        using var lightfx_wrapper_64 = new BinaryWriter(new FileStream(path, FileMode.Create));
        lightfx_wrapper_64.Write(Properties.Resources.Aurora_LightFXWrapper64);

        return true;
    }

    private bool UninstallWrapper()
    {
        var installpath = SteamUtils.GetGamePath(232090);
        if (string.IsNullOrWhiteSpace(installpath)) return false;
        var path = Path.Combine(installpath, "Binaries", "Win64", "LightFX.dll");

        if (File.Exists(path))
            File.Delete(path);

        return true;
    }
}