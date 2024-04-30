using System.IO;
using System.Windows;
using System.Windows.Forms;
using AuroraRgb.Settings;
using AuroraRgb.Utils.Steam;
using MessageBox = System.Windows.MessageBox;

namespace AuroraRgb.Profiles.HotlineMiami;

/// <summary>
/// Interaction logic for Control_HM.xaml
/// </summary>
public partial class Control_HM
{
    private Application profile_manager;

    public Control_HM(Application profile)
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

    private void patch_drm_button_Click(object? sender, RoutedEventArgs e)
    {
        var dialog = new FolderBrowserDialog();
        var result = dialog.ShowDialog();

        if (result != DialogResult.OK) return;
        if (InstallWrapper(dialog.SelectedPath))
            MessageBox.Show("Aurora Wrapper Patch for LightFX applied to\r\n" + dialog.SelectedPath);
        else
            MessageBox.Show("Aurora LightFX Wrapper could not be installed.\r\nGame is not installed.");
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
            installPath = SteamUtils.GetGamePath(219150);


        if (string.IsNullOrWhiteSpace(installPath)) return false;
        var path = Path.Combine(installPath, "LightFX.dll");

        if (!File.Exists(path))
            Directory.CreateDirectory(Path.GetDirectoryName(path));

        using var lightfxWrapper86 = new BinaryWriter(new FileStream(path, FileMode.Create));
        lightfxWrapper86.Write(Properties.Resources.Aurora_LightFXWrapper86);

        return true;

    }

    private bool UninstallWrapper()
    {
        var installPath = SteamUtils.GetGamePath(219150);
        if (string.IsNullOrWhiteSpace(installPath)) return false;
        var path = Path.Combine(installPath, "LightFX.dll");

        if (File.Exists(path))
            File.Delete(path);

        return true;

    }
}