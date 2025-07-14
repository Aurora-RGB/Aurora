using System;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;
using AuroraRgb.Devices;
using AuroraRgb.Modules.Layouts;
using MessageBox = System.Windows.MessageBox;

namespace AuroraRgb.Settings.Controls;

public partial class Control_SettingsDevicesAndWrappers
{
    private readonly Task<DeviceManager> _deviceManager;
    private readonly Task<KeyboardLayoutManager> _layoutManager;

    public Control_SettingsDevicesAndWrappers(Task<DeviceManager> deviceManager, Task<KeyboardLayoutManager> layoutManager)
    {
        _deviceManager = deviceManager;
        _layoutManager = layoutManager;

        InitializeComponent();
    }

    private void ResetDevices(object? sender, RoutedEventArgs e) => Task.Run(async () => await (await _deviceManager).ResetDevices());

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

    private async void LayoutsRefreshButton_OnClick(object sender, RoutedEventArgs e)
    {
        var keyboardLayoutManager = await _layoutManager;
        await keyboardLayoutManager.LoadBrandDefault();
    }
}