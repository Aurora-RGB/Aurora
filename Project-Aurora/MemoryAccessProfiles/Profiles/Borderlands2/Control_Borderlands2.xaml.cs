using System.Windows;
using System.Windows.Controls;
using MemoryAccessProfiles.Profiles.Borderlands2.GSI;
using Application = AuroraRgb.Profiles.Application;

namespace MemoryAccessProfiles.Profiles.Borderlands2;

/// <summary>
/// Interaction logic for Control_Borderlands2.xaml
/// </summary>
public partial class Control_Borderlands2
{
    private readonly Application _profileManager;

    public Control_Borderlands2(Application profile)
    {
        InitializeComponent();

        _profileManager = profile;
    }

    private void preview_health_amount_ValueChanged(object? sender, RoutedPropertyChangedEventArgs<double> e)
    {
        if (sender is not Slider slider) return;
        preview_health_amount_label.Text = (int)slider.Value + "%";

        if (!IsLoaded || _profileManager.Config.Event._game_state is not GameState_Borderlands2 gameState) return;
        gameState.Player.CurrentHealth = (float)slider.Value;
        gameState.Player.MaximumHealth = 100.0f;
    }

    private void preview_shield_amount_ValueChanged(object? sender, RoutedPropertyChangedEventArgs<double> e)
    {
        if (sender is not Slider slider) return;
        preview_shield_amount_label.Text = (int)slider.Value + "%";

        if (!IsLoaded || _profileManager.Config.Event._game_state is not GameState_Borderlands2 gameState) return;
        gameState.Player.CurrentShield = (float)slider.Value;
        gameState.Player.MaximumShield = 100.0f;
    }
}