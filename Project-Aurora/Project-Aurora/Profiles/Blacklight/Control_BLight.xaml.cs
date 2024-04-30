using System;
using System.IO;
using System.Windows;
using AuroraRgb.Settings;
using AuroraRgb.Utils.Steam;

namespace AuroraRgb.Profiles.Blacklight
{
    /// <summary>
    /// Interaction logic for Control_BLight.xaml
    /// </summary>
    public partial class Control_BLight
    {
        private Application profile_manager;

        public Control_BLight(Application profile)
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
            if (String.IsNullOrWhiteSpace(installpath))
                installpath = SteamUtils.GetGamePath(209870);

            if (!String.IsNullOrWhiteSpace(installpath))
            {
                //86
                string path = System.IO.Path.Combine(installpath, "Binaries", "Win32", "LightFX.dll");

                if (!File.Exists(path))
                    Directory.CreateDirectory(System.IO.Path.GetDirectoryName(path));

                using (BinaryWriter lightfx_wrapper_86 = new BinaryWriter(new FileStream(path, FileMode.Create)))
                {
                    lightfx_wrapper_86.Write(Properties.Resources.Aurora_LightFXWrapper86);
                }

                return true;
            }
            else
            {
                return false;
            }
        }

        private bool UninstallWrapper(string installpath = "")
        {
            if (String.IsNullOrWhiteSpace(installpath))
                installpath = SteamUtils.GetGamePath(209870);

            if (!String.IsNullOrWhiteSpace(installpath))
            {
                //86
                string path = System.IO.Path.Combine(installpath, "Binaries", "Win32", "LightFX.dll");

                if (File.Exists(path))
                    File.Delete(path);

                return true;
            }
            else
            {
                return false;
            }
        }
    }
}
