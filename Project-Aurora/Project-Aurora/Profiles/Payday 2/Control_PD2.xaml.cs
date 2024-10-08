﻿using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using AuroraRgb.Profiles.Payday_2.GSI;
using AuroraRgb.Profiles.Payday_2.GSI.Nodes;

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

    private async void install_mod_button_Click(object? sender, RoutedEventArgs e)
    {
        InstallModButton.IsEnabled = false;
        var errorMessage = await Pd2GsiUtils.InstallMod();
        if (errorMessage != null)
        {
            MessageBox.Show(errorMessage);
        }
        else
        {
            MessageBox.Show("GSI for Payday 2 installed.");
        }
        InstallModButton.IsEnabled = true;
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