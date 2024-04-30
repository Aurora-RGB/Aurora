using System;
using System.IO;
using System.Windows;
using System.Windows.Forms;
using MessageBox = System.Windows.MessageBox;

namespace AuroraRgb.Profiles.Guild_Wars_2;

/// <summary>
/// Interaction logic for Control_GW2.xaml
/// </summary>
public partial class Control_GW2
{
    public Control_GW2(Application _)
    {
        InitializeComponent();
    }

    private void patch_64bit_button_Click(object? sender, RoutedEventArgs e)
    {
        var dialog = new FolderBrowserDialog();
        DialogResult result = dialog.ShowDialog();

        if (result != DialogResult.OK) return;
        if (InstallWrapper(dialog.SelectedPath))
            MessageBox.Show("Aurora Wrapper Patch for LightFX applied to\r\n" + dialog.SelectedPath);
        else
            MessageBox.Show("Aurora LightFX Wrapper could not be installed.");
    }

    private bool InstallWrapper(string installPath = "")
    {
        if (string.IsNullOrWhiteSpace(installPath)) return false;
        var path = Path.Combine(installPath, "LightFX.dll");

        if (!File.Exists(path))
            Directory.CreateDirectory(Path.GetDirectoryName(path));

        using var lightfxWrapper = new BinaryWriter(new FileStream(path, FileMode.Create));
        lightfxWrapper.Write( Properties.Resources.Aurora_LightFXWrapper64 );

        return true;

    }
}