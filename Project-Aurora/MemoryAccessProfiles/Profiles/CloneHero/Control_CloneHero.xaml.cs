using System.Windows;
using System.Windows.Controls;
using MemoryAccessProfiles.Profiles.CloneHero.GSI;
using Xceed.Wpf.Toolkit;
using Application = AuroraRgb.Profiles.Application;

namespace MemoryAccessProfiles.Profiles.CloneHero;

/// <summary>
/// Interaction logic for Control_CloneHero.xaml
/// </summary>
public partial class Control_CloneHero
{
    private readonly Application _profileManager;

    public Control_CloneHero(Application profile)
    {
        InitializeComponent();

        _profileManager = profile;
    }

    private void preview_streak_amount_ValueChanged(object? sender, RoutedPropertyChangedEventArgs<object> e)
    {
        if (sender is not IntegerUpDown { Value: not null } integerUpDown) return;
        if (!IsLoaded || _profileManager.Config.Event.GameState is not GameState_CloneHero gameState) return;
        gameState.Player.NoteStreak = integerUpDown.Value.Value;

        #region NoteStreak Extras

        // Breaks up the note streak into the 1x, 2x, 3x, 4x zones for easy lighting options
        var streak = integerUpDown.Value.Value;
        switch (streak)
        {
            case >= 0 and <= 10:
            {
                gameState.Player.NoteStreak1x = streak;
                gameState.Player.NoteStreak2x = 0;
                gameState.Player.NoteStreak3x = 0;
                gameState.Player.NoteStreak4x = 0;

                // This accounts for how CH changes the color once the bar fills up
                if (streak == 10)
                {
                    gameState.Player.NoteStreak2x = 10;
                }

                break;
            }
            case > 10 and <= 20:
            {
                gameState.Player.NoteStreak1x = 0;
                gameState.Player.NoteStreak2x = streak - 10;
                gameState.Player.NoteStreak3x = 0;
                gameState.Player.NoteStreak4x = 0;

                // This accounts for how CH changes the color once the bar fills up
                if (streak == 20)
                {
                    gameState.Player.NoteStreak3x = 10;
                }

                break;
            }
            case > 20 and <= 30:
            {
                gameState.Player.NoteStreak1x = 0;
                gameState.Player.NoteStreak2x = 0;
                gameState.Player.NoteStreak3x = streak - 20;
                gameState.Player.NoteStreak4x = 0;

                // This accounts for how CH changes the color once the bar fills up
                if (streak == 30)
                {
                    gameState.Player.NoteStreak4x = 10;
                }

                break;
            }
            case > 30 and <= 40:
                gameState.Player.NoteStreak1x = 0;
                gameState.Player.NoteStreak2x = 0;
                gameState.Player.NoteStreak3x = 0;
                gameState.Player.NoteStreak4x = streak - 30;
                break;
        }

        #endregion
    }

    private void preview_sp_percent_ValueChanged(object? sender, RoutedPropertyChangedEventArgs<double> e)
    {
        if (sender is not Slider slider) return;
        preview_sp_percent_label.Text = (int)slider.Value + "%";

        if (!IsLoaded || _profileManager.Config.Event.GameState is not GameState_CloneHero gameState) return;
        gameState.Player.StarPowerPercent = (int)slider.Value;
    }

    private void preview_sp_active(object? sender, RoutedEventArgs e)
    {
        preview_sp_enabled_label.Text = "true";
        if (!IsLoaded || _profileManager.Config.Event.GameState is not GameState_CloneHero gameState) return;
        gameState.Player.IsStarPowerActive = true;
    }

    private void preview_sp_deactive(object? sender, RoutedEventArgs e)
    {
        preview_sp_enabled_label.Text = "false";
        if (!IsLoaded || _profileManager.Config.Event.GameState is not GameState_CloneHero gameState) return;
        gameState.Player.IsStarPowerActive = false;
    }

    private void preview_menu_active(object? sender, RoutedEventArgs e)
    {
        preview_menu_enabled_label.Text = "true";
        if (!IsLoaded || _profileManager.Config.Event.GameState is not GameState_CloneHero gameState) return;
        gameState.Player.IsAtMenu = true;
    }

    private void preview_menu_deactive(object? sender, RoutedEventArgs e)
    {
        preview_menu_enabled_label.Text = "false";
        if (!IsLoaded || _profileManager.Config.Event.GameState is not GameState_CloneHero gameState) return;
        gameState.Player.IsAtMenu = false;
    }

    private void preview_fc_active(object? sender, RoutedEventArgs e)
    {
        preview_fc_enabled_label.Text = "true";
        if (!IsLoaded || _profileManager.Config.Event.GameState is not GameState_CloneHero gameState) return;
        gameState.Player.IsFC = true;
    }

    private void preview_fc_deactive(object? sender, RoutedEventArgs e)
    {
        preview_fc_enabled_label.Text = "false";
        if (!IsLoaded || _profileManager.Config.Event.GameState is not GameState_CloneHero gameState) return;
        gameState.Player.IsFC = false;
    }

    private void preview_notes_total_ValueChanged(object? sender, RoutedPropertyChangedEventArgs<object> e)
    {
        if (sender is not IntegerUpDown { Value: not null } integerUpDown) return;
        if (!IsLoaded || _profileManager.Config.Event.GameState is not GameState_CloneHero gameState) return;
        gameState.Player.NotesTotal = integerUpDown.Value.Value;
    }

    private void preview_score_ValueChanged(object? sender, RoutedPropertyChangedEventArgs<object> e)
    {
        if (sender is not IntegerUpDown { Value: not null } integerUpDown) return;
        if (!IsLoaded || _profileManager.Config.Event.GameState is not GameState_CloneHero gameState) return;
        gameState.Player.Score = integerUpDown.Value.Value;
    }

    #region frets

    // Green
    private void preview_green_active(object? sender, RoutedEventArgs e)
    {
        if (!IsLoaded || _profileManager.Config.Event.GameState is not GameState_CloneHero gameState) return;
        gameState.Player.IsGreenPressed = true;
    }

    private void preview_green_deactive(object? sender, RoutedEventArgs e)
    {
        if (!IsLoaded || _profileManager.Config.Event.GameState is not GameState_CloneHero gameState) return;
        gameState.Player.IsGreenPressed = false;
    }

    // Red
    private void preview_red_active(object? sender, RoutedEventArgs e)
    {
        if (!IsLoaded || _profileManager.Config.Event.GameState is not GameState_CloneHero gameState) return;
        gameState.Player.IsRedPressed = true;
    }

    private void preview_red_deactive(object? sender, RoutedEventArgs e)
    {
        if (!IsLoaded || _profileManager.Config.Event.GameState is not GameState_CloneHero gameState) return;
        gameState.Player.IsRedPressed = false;
    }

    // Yellow
    private void preview_yellow_active(object? sender, RoutedEventArgs e)
    {
        if (!IsLoaded || _profileManager.Config.Event.GameState is not GameState_CloneHero gameState) return;
        gameState.Player.IsYellowPressed = true;
    }

    private void preview_yellow_deactive(object? sender, RoutedEventArgs e)
    {
        if (!IsLoaded || _profileManager.Config.Event.GameState is not GameState_CloneHero gameState) return;
        gameState.Player.IsYellowPressed = false;
    }

    // Blue
    private void preview_blue_active(object? sender, RoutedEventArgs e)
    {
        if (!IsLoaded || _profileManager.Config.Event.GameState is not GameState_CloneHero gameState) return;
        gameState.Player.IsBluePressed = true;
    }

    private void preview_blue_deactive(object? sender, RoutedEventArgs e)
    {
        if (!IsLoaded || _profileManager.Config.Event.GameState is not GameState_CloneHero gameState) return;
        gameState.Player.IsBluePressed = false;
    }

    // Orange
    private void preview_orange_active(object? sender, RoutedEventArgs e)
    {
        if (!IsLoaded || _profileManager.Config.Event.GameState is not GameState_CloneHero gameState) return;
        gameState.Player.IsOrangePressed = true;
    }

    private void preview_orange_deactive(object? sender, RoutedEventArgs e)
    {
        if (!IsLoaded || _profileManager.Config.Event.GameState is not GameState_CloneHero gameState) return;
        gameState.Player.IsOrangePressed = false;
    }

    #endregion
}