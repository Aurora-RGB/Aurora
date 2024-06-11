using System.Windows;
using System.Windows.Controls;
using MemoryAccessProfiles.Profiles.ResidentEvil2.GSI;
using MemoryAccessProfiles.Profiles.ResidentEvil2.GSI.Nodes;
using Xceed.Wpf.Toolkit;
using Application = AuroraRgb.Profiles.Application;

namespace MemoryAccessProfiles.Profiles.ResidentEvil2;

/// <summary>
/// Interaction logic for Control_ResidentEvil2.xaml
/// </summary>
public partial class Control_ResidentEvil2
{
    private readonly Application _profileManager;

    public Control_ResidentEvil2(Application profile)
    {
        InitializeComponent();

        _profileManager = profile;

        SetSettings();

        _profileManager.ProfileChanged += Profile_manager_ProfileChanged;
    }

    private void Profile_manager_ProfileChanged(object? sender, EventArgs e)
    {
        SetSettings();
    }

    private void SetSettings()
    {
        if (preview_status.HasItems) return;
        preview_status.Items.Add(Player_ResidentEvil2.PlayerStatus.Fine);
        preview_status.Items.Add(Player_ResidentEvil2.PlayerStatus.LiteFine);
        preview_status.Items.Add(Player_ResidentEvil2.PlayerStatus.Caution);
        preview_status.Items.Add(Player_ResidentEvil2.PlayerStatus.Danger);
        preview_status.Items.Add(Player_ResidentEvil2.PlayerStatus.Dead);
        preview_status.Items.Add(Player_ResidentEvil2.PlayerStatus.OffGame);
    }

    private void preview_status_SelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        if (!IsLoaded || _profileManager.Config.Event.GameState is not GameState_ResidentEvil2 gameState) return;
        gameState.Player.Status = (Player_ResidentEvil2.PlayerStatus)preview_status.SelectedItem;
    }

    private void preview_poison_Checked(object? sender, RoutedEventArgs e)
    {
        if (!IsLoaded || _profileManager.Config.Event.GameState is not GameState_ResidentEvil2 gameState ||
            sender is not CheckBox { IsChecked: not null } checkBox) return;
        gameState.Player.Poison = checkBox.IsChecked.Value;
    }

    private void preview_rank_ValueChanged(object? sender, RoutedPropertyChangedEventArgs<object> e)
    {
        if (!IsLoaded || _profileManager.Config.Event.GameState is not GameState_ResidentEvil2 gameState ||
            sender is not IntegerUpDown { Value: not null } integerUpDown) return;
        gameState.Player.Rank = integerUpDown.Value.Value;
    }
}