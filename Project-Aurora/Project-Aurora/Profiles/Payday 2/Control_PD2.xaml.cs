using System.Diagnostics;
using System.IO;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using AuroraRgb.Profiles.Payday_2.GSI;
using AuroraRgb.Profiles.Payday_2.GSI.Nodes;
using AuroraRgb.Utils.Steam;
using ICSharpCode.SharpZipLib.Zip;

namespace AuroraRgb.Profiles.Payday_2;

/// <summary>
/// Interaction logic for Control_PD2.xaml
/// </summary>
public partial class Pd2
{
    private readonly Application _profileManager;

    public Pd2(Application profile)
    {
        InitializeComponent();

        _profileManager = profile;
    }

    private void get_hook_button_Click(object? sender, RoutedEventArgs e)
    {
        Process.Start("explorer", @"https://superblt.znix.xyz/");
    }

    private void install_mod_button_Click(object? sender, RoutedEventArgs e)
    {
        var pd2Path = SteamUtils.GetGamePath(218620);

        if (string.IsNullOrWhiteSpace(pd2Path))
        {
            MessageBox.Show("Payday 2 is not installed through Steam.\r\nCould not install the GSI mod.");
            return;
        }

        if (!Directory.Exists(pd2Path))
        {
            MessageBox.Show("Payday 2 directory is not found.\r\nCould not install the GSI mod.");
            return;
        }

        if (!Directory.Exists(Path.Combine(pd2Path, "mods")))
        {
            MessageBox.Show("BLT Hook was not found.\r\nCould not install the GSI mod.");
            return;
        }

        //copy gsi config file
        using var gsiConfigFile = new MemoryStream(Properties.Resources.PD2_GSI);
        File.WriteAllBytes(Path.Combine(pd2Path, "GSI", "Aurora.xml"), gsiConfigFile.ToArray());
        
        var modFolder = Path.Combine(pd2Path, "mods", "GSI");

        const string zipUrl = "https://github.com/Aurora-RGB/Payday2-GSI/archive/refs/heads/main.zip";
        using var webClient = new WebClient();
        using var zipStream = new MemoryStream(webClient.DownloadData(zipUrl));
        using var zipInputStream = new ZipInputStream(zipStream);
        while (zipInputStream.GetNextEntry() is { } entry)
        {
            if (!entry.IsFile)
                continue;

            var entryName = entry.Name;
            var fullPath = Path.Combine(modFolder, entryName).Replace("\\Payday2-GSI-main", "");

            Directory.CreateDirectory(Path.GetDirectoryName(fullPath));

            using var entryFileStream = File.Create(fullPath);
            zipInputStream.CopyTo(entryFileStream);
        }

        MessageBox.Show("GSI for Payday 2 installed.");
    }

    private void preview_gamestate_SelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        if (!IsLoaded) return;
        (_profileManager.Config.Event.GameState as GameState_PD2).Game.State = (GameStates)preview_gamestate.SelectedValue;
    }

    private void preview_levelphase_SelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        if (!IsLoaded) return;
        (_profileManager.Config.Event.GameState as GameState_PD2).Level.Phase = (LevelPhase)preview_levelphase.SelectedValue;
    }

    private void preview_playerstate_SelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        if (!IsLoaded) return;
        (_profileManager.Config.Event.GameState as GameState_PD2).Players.LocalPlayer.State = (PlayerState)preview_playerstate.SelectedValue;
    }

    private void preview_health_slider_ValueChanged(object? sender, RoutedPropertyChangedEventArgs<double> e)
    {
        var hpVal = (int)preview_health_slider.Value;
        if (preview_health_amount is not Label) return;
        preview_health_amount.Content = hpVal + "%";
        (_profileManager.Config.Event.GameState as GameState_PD2).Players.LocalPlayer.Health.Current = hpVal;
        (_profileManager.Config.Event.GameState as GameState_PD2).Players.LocalPlayer.Health.Max = 100;
    }

    private void preview_ammo_slider_ValueChanged(object? sender, RoutedPropertyChangedEventArgs<double> e)
    {
        var ammoVal = (int)preview_ammo_slider.Value;
        if (preview_ammo_amount is not Label) return;
        preview_ammo_amount.Content = ammoVal + "%";
        (_profileManager.Config.Event.GameState as GameState_PD2).Players.LocalPlayer.SelectedWeapon.Current_Clip = ammoVal;
        (_profileManager.Config.Event.GameState as GameState_PD2).Players.LocalPlayer.SelectedWeapon.Max_Clip = 100;
    }

    private void preview_suspicion_slider_ValueChanged(object? sender, RoutedPropertyChangedEventArgs<double> e)
    {
        var suspVal = (float)preview_suspicion_slider.Value;
        if (preview_suspicion_amount is not Label) return;
        preview_suspicion_amount.Content = (int)suspVal + "%";
        (_profileManager.Config.Event.GameState as GameState_PD2).Players.LocalPlayer.SuspicionAmount = suspVal / 100.0f;
    }

    private void preview_flashbang_slider_ValueChanged(object? sender, RoutedPropertyChangedEventArgs<double> e)
    {
        var flashVal = (float)preview_flashbang_slider.Value;
        if (preview_flashbang_amount is not Label) return;
        preview_flashbang_amount.Content = (int)flashVal + "%";
        (_profileManager.Config.Event.GameState as GameState_PD2).Players.LocalPlayer.FlashAmount = flashVal / 100.0f;
    }

    private void preview_swansong_Checked(object? sender, RoutedEventArgs e)
    {
        if (!IsLoaded || sender is not CheckBox { IsChecked: not null } checkBox) return;
        (_profileManager.Config.Event.GameState as GameState_PD2).Players.LocalPlayer.SwanSong = checkBox.IsChecked.Value;
    }

    private void get_lib_button_Click(object? sender, RoutedEventArgs e)
    {
        Process.Start("explorer", @"https://github.com/simon-wh/PAYDAY-2-BeardLib");
    }
}