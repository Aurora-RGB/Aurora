using System;
using System.IO;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using AuroraRgb.Profiles.CSGO.GSI;
using AuroraRgb.Profiles.CSGO.GSI.Nodes;
using AuroraRgb.Utils.Steam;
using Timer = System.Timers.Timer;

namespace AuroraRgb.Profiles.CSGO;

/// <summary>
/// Interaction logic for Control_CSGO.xaml
/// </summary>
public partial class Control_CSGO
{
    private readonly CSGO _profileManager;

    private readonly Timer _previewBombTimer;
    private readonly Timer _previewBombRemoveEffectTimer;

    private int _previewKills;
    private int _previewKillshs;

    public Control_CSGO(Application profile)
    {
        InitializeComponent();

        _profileManager = (CSGO)profile;

        SetSettings();

        _previewBombTimer = new Timer(45000);
        _previewBombTimer.Elapsed += preview_bomb_timer_Tick;

        _previewBombRemoveEffectTimer = new Timer(5000);
        _previewBombRemoveEffectTimer.Elapsed += preview_bomb_remove_effect_timer_Tick;

        _profileManager.ProfileChanged += Profile_manager_ProfileChanged;
    }

    private void Profile_manager_ProfileChanged(object? sender, EventArgs e)
    {
        SetSettings();
    }

    private void SetSettings()
    {
        preview_team.Items.Clear();
        preview_team.Items.Add(PlayerTeam.Undefined);
        preview_team.Items.Add(PlayerTeam.CT);
        preview_team.Items.Add(PlayerTeam.T);
        preview_team.SelectedItem = PlayerTeam.Undefined;
    }

    private void preview_bomb_timer_Tick(object? sender, EventArgs e)
    {
        Dispatcher.Invoke(
            () =>
            {
                if (_profileManager.Config.Event.GameState is not GameStateCsgo gameState)
                {
                    return;
                }
                preview_bomb_defused.IsEnabled = false;
                preview_bomb_start.IsEnabled = true;

                gameState.Round.Bomb = BombState.Exploded;
                _previewBombTimer.Stop();

                _previewBombRemoveEffectTimer.Start();
            });
    }

    private void preview_bomb_remove_effect_timer_Tick(object? sender, EventArgs e)
    {
        (_profileManager.Config.Event.GameState as GameStateCsgo).Round.Bomb = BombState.Undefined;
        _previewBombRemoveEffectTimer.Stop();
    }

    //Overview

    private async void patch_button_Click(object? sender, RoutedEventArgs e)
    {
        var result = await _profileManager.InstallGsi();;
        if (result)
            MessageBox.Show("Aurora GSI Config file installed successfully.");
        else
            MessageBox.Show("Aurora GSI Config file could not be installed.\r\nGame is not installed.");
    }

    private void unpatch_button_Click(object? sender, RoutedEventArgs e)
    {
        if (UninstallGSI())
            MessageBox.Show("Aurora GSI Config file uninstalled successfully.");
        else
            MessageBox.Show("Aurora GSI Config file could not be uninstalled.\r\nGame is not installed.");
    }

    ////Preview

    private void preview_team_SelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        if (preview_team.Items.Count == 0)
            return;
        if (_profileManager.Config.Event.GameState is not GameStateCsgo eventGameState)
        {
            return;
        }
        eventGameState.Player.Team = (PlayerTeam)preview_team.SelectedItem;
        eventGameState.Round.Phase = (PlayerTeam)preview_team.SelectedItem == PlayerTeam.Undefined ? RoundPhase.Undefined : RoundPhase.Live;
    }

    private void preview_health_slider_ValueChanged(object? sender, RoutedPropertyChangedEventArgs<double> e)
    {
        if (_profileManager.Config.Event.GameState is not GameStateCsgo gameState)
        {
            return;
        }
        var hp_val = (int)preview_health_slider.Value;
        if (preview_health_amount != null)
        {
            preview_health_amount.Content = hp_val + "%";
            gameState.Player.State.Health = hp_val;
        }
    }

    private void preview_ammo_slider_ValueChanged(object? sender, RoutedPropertyChangedEventArgs<double> e)
    {
        if (_profileManager.Config.Event.GameState is not GameStateCsgo gameState)
        {
            return;
        }
        var ammo_val = (int)preview_ammo_slider.Value;
        if (preview_ammo_amount != null)
        {
            preview_ammo_amount.Content = ammo_val + "%";
            gameState.Player.Weapons.ActiveWeapon.AmmoClip = ammo_val;
            gameState.Player.Weapons.ActiveWeapon.AmmoClipMax = 100;
        }
    }

    private void preview_flash_slider_ValueChanged(object? sender, RoutedPropertyChangedEventArgs<double> e)
    {
        var flash_val = (int)preview_flash_slider.Value;
        if (preview_flash_amount != null)
        {
            preview_flash_amount.Content = flash_val + "%";
            (_profileManager.Config.Event.GameState as GameStateCsgo).Player.State.Flashed = (int)(flash_val / 100.0 * 255.0);
        }
    }

    private void preview_burning_slider_ValueChanged(object? sender, RoutedPropertyChangedEventArgs<double> e)
    {
        var burning_val = (int)preview_burning_slider.Value;
        if (preview_burning_amount != null)
        {
            preview_burning_amount.Content = burning_val + "%";
            (_profileManager.Config.Event.GameState as GameStateCsgo).Player.State.Burning = (int)(burning_val / 100.0 * 255.0);
        }
    }

    private void preview_bomb_start_Click(object? sender, RoutedEventArgs e)
    {
        preview_bomb_defused.IsEnabled = true;
        preview_bomb_start.IsEnabled = false;

        (_profileManager.Config.Event.GameState as GameStateCsgo).Round.Bomb = BombState.Planted;
        _previewBombTimer.Start();
        _previewBombRemoveEffectTimer.Stop();
    }

    private void preview_bomb_defused_Click(object? sender, RoutedEventArgs e)
    {
        preview_bomb_defused.IsEnabled = false;
        preview_bomb_start.IsEnabled = true;

        (_profileManager.Config.Event.GameState as GameStateCsgo).Round.Bomb = BombState.Defused;
        _previewBombTimer.Stop();
        _previewBombRemoveEffectTimer.Start();
    }

    private void preview_typing_enabled_Checked(object? sender, RoutedEventArgs e)
    {
        if (_profileManager.Config.Event.GameState is not GameStateCsgo gameState)
        {
            return;
        }
        gameState.Player.Activity = preview_typing_enabled.IsChecked.HasValue &&
                                    preview_typing_enabled.IsChecked.Value ? PlayerActivity.TextInput : PlayerActivity.Undefined;
    }

    private void preview_respawn_Click(object? sender, RoutedEventArgs e)
    {
        if (_profileManager.Config.Event.GameState is not GameStateCsgo gameState)
        {
            return;
        }
        gameState.Provider.SteamID = gameState.Player.SteamID;

        gameState.Player.State.Health = 100;
        gameState.Previously.Player.State.Health = 99;

        var curr_hp_val = (int)preview_health_slider.Value;

        System.Threading.Timer reset_conditions_timer = null;
        reset_conditions_timer = new System.Threading.Timer(obj =>
            {
                (_profileManager.Config.Event.GameState as GameStateCsgo).Player.State.Health = curr_hp_val;
                (_profileManager.Config.Event.GameState as GameStateCsgo).Previously.Player.State.Health = 100;

                reset_conditions_timer.Dispose();
            },
            null, 500, Timeout.Infinite);
    }

    private void preview_addkill_hs_Click(object? sender, RoutedEventArgs e)
    {
        if (_profileManager.Config.Event.GameState is not GameStateCsgo gameState)
        {
            return;
        }
        gameState.Provider.SteamID = gameState.Player.SteamID;

        gameState.Previously.Player.State.RoundKills = _previewKills;
        gameState.Player.State.RoundKills = ++_previewKills;
        gameState.Previously.Player.State.RoundKillHS = _previewKillshs;
        gameState.Player.State.RoundKillHS = ++_previewKillshs;

        System.Threading.Timer resetConditionsTimer = null;
        resetConditionsTimer = new System.Threading.Timer(obj =>
            {
                gameState.Previously.Player.State.RoundKills = _previewKills;
                gameState.Player.State.RoundKills = _previewKills;
                gameState.Previously.Player.State.RoundKillHS = _previewKillshs;
                gameState.Player.State.RoundKillHS = _previewKillshs;

                resetConditionsTimer.Dispose();
            },
            null, 500, Timeout.Infinite);

        preview_kills_label.Text = $"Kills: {_previewKills} Headshots: {_previewKillshs}";
    }

    private void preview_addkill_Click(object? sender, RoutedEventArgs e)
    {
        if (_profileManager.Config.Event.GameState is not GameStateCsgo gameState)
        {
            return;
        }
        gameState.Provider.SteamID = gameState.Player.SteamID;

        gameState.Previously.Player.State.RoundKills = _previewKills;
        gameState.Player.State.RoundKills = ++_previewKills;

        System.Threading.Timer resetConditionsTimer = null;
        resetConditionsTimer = new System.Threading.Timer(obj =>
            {
                (_profileManager.Config.Event.GameState as GameStateCsgo).Previously.Player.State.RoundKills = _previewKills;
                (_profileManager.Config.Event.GameState as GameStateCsgo).Player.State.RoundKills = _previewKills;

                resetConditionsTimer.Dispose();
            },
            null, 500, Timeout.Infinite);

        preview_kills_label.Text = $"Kills: {_previewKills} Headshots: {_previewKillshs}";
    }
    private void preview_kills_reset_Click(object? sender, RoutedEventArgs e)
    {
        if (_profileManager.Config.Event.GameState is not GameStateCsgo gameState)
        {
            return;
        }
        gameState.Provider.SteamID = gameState.Player.SteamID;

        gameState.Previously.Player.State.RoundKills = _previewKills;
        gameState.Player.State.RoundKills = 0;
        gameState.Previously.Player.State.RoundKillHS = _previewKillshs;
        gameState.Player.State.RoundKillHS = 0;

        System.Threading.Timer reset_conditions_timer = null;
        reset_conditions_timer = new System.Threading.Timer(obj =>
            {
                (_profileManager.Config.Event.GameState as GameStateCsgo).Previously.Player.State.RoundKills = 0;
                (_profileManager.Config.Event.GameState as GameStateCsgo).Player.State.RoundKills = 0;
                (_profileManager.Config.Event.GameState as GameStateCsgo).Previously.Player.State.RoundKillHS = 0;
                (_profileManager.Config.Event.GameState as GameStateCsgo).Player.State.RoundKillHS = 0;

                reset_conditions_timer.Dispose();
            },
            null, 500, Timeout.Infinite);

        _previewKills = 0;
        _previewKillshs = 0;

        preview_kills_label.Text = $"Kills: {_previewKills} Headshots: {_previewKillshs}";
    }

    private bool UninstallGSI()
    {
        var installPath = SteamUtils.GetGamePath(730);
        if (string.IsNullOrWhiteSpace(installPath)) return false;
        var path = Path.Combine(installPath, "game", "csgo", "cfg", "gamestate_integration_aurora.cfg");

        if (File.Exists(path))
            File.Delete(path);

        return true;
    }
}