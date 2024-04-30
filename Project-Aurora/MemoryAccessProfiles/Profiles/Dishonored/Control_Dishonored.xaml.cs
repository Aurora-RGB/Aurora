using System.Windows;
using System.Windows.Controls;
using MemoryAccessProfiles.Profiles.Dishonored.GSI;
using Xceed.Wpf.Toolkit;
using Application = AuroraRgb.Profiles.Application;

namespace MemoryAccessProfiles.Profiles.Dishonored;

/// <summary>
/// Interaction logic for Control_Dishonored.xaml
/// </summary>
public partial class Control_Dishonored
{
    private readonly Application _profileManager;

    public Control_Dishonored(Application profile)
    {
        InitializeComponent();

        _profileManager = profile;
    }

    private void preview_health_amount_ValueChanged(object? sender, RoutedPropertyChangedEventArgs<double> e)
    {
        if (sender is not Slider slider) return;
        preview_health_amount_label.Text = (int)slider.Value + "%";

        if (!IsLoaded || _profileManager.Config.Event._game_state is not GameState_Dishonored gameState) return;
        gameState.Player.CurrentHealth = (int)slider.Value;
        gameState.Player.MaximumHealth = 100;
    }

    private void preview_mana_amount_ValueChanged(object? sender, RoutedPropertyChangedEventArgs<double> e)
    {
        if (sender is not Slider slider) return;
        preview_mana_amount_label.Text = (int)slider.Value + "%";

        if (!IsLoaded || _profileManager.Config.Event._game_state is not GameState_Dishonored gameState) return;
        gameState.Player.CurrentMana = (int)slider.Value;
        gameState.Player.MaximumMana = 100;
    }

    private void preview_manapots_ValueChanged(object? sender, RoutedPropertyChangedEventArgs<object> e)
    {
        if (!IsLoaded || sender is not IntegerUpDown { Value: not null } integerUpDown ||
            _profileManager.Config.Event._game_state is not GameState_Dishonored gameState) return;
        gameState.Player.ManaPots = integerUpDown.Value.Value;
    }

    private void preview_healthpots_ValueChanged(object? sender, RoutedPropertyChangedEventArgs<object> e)
    {
        if (!IsLoaded || sender is not IntegerUpDown { Value: not null } integerUpDown ||
            _profileManager.Config.Event._game_state is not GameState_Dishonored gameState) return;
        gameState.Player.HealthPots = integerUpDown.Value.Value;
    }
}