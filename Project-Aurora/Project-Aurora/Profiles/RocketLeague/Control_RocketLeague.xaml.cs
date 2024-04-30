using System;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using AuroraRgb.Profiles.RocketLeague.GSI;
using Xceed.Wpf.Toolkit;

namespace AuroraRgb.Profiles.RocketLeague;

/// <summary>
/// Interaction logic for Control_RocketLeague.xaml
/// </summary>
public partial class Control_RocketLeague
{
    private readonly Application _profileManager;

    public Control_RocketLeague(Application profile)
    {
        _profileManager = profile;

        InitializeComponent();

        SetSettings();

        _profileManager.ProfileChanged += Profile_manager_ProfileChanged;
    }

    private void Profile_manager_ProfileChanged(object? sender, EventArgs e)
    {
        SetSettings();
    }

    private void SetSettings()
    {
        if (!preview_team.HasItems)
        {
            preview_team.DisplayMemberPath = "Text";
            preview_team.SelectedValuePath = "Value";
            preview_team.Items.Add(new { Text = "Spectator", Value = -1});
            preview_team.Items.Add(new { Text = "Blue", Value = 0 });
            preview_team.Items.Add(new { Text = "Orange", Value = 1 });
            preview_team.SelectedIndex = 1;
        }

        if (!preview_status.HasItems)
        {
            preview_status.ItemsSource = Enum.GetValues(typeof(RLStatus)).Cast<RLStatus>();
            preview_status.SelectedIndex = (int)RLStatus.InGame;
        }

        ColorPicker_team1.SelectedColor = Colors.Blue;
        ColorPicker_team2.SelectedColor = Colors.Orange;
        preview_team1_score.Value = 0;
        preview_team2_score.Value = 0;
    }

    private void Button_DownloadBakkesMod(object? sender, RoutedEventArgs e)
    {
        Process.Start("explorer", @"https://bakkesmod.com/index.php");
    }

    private void Button_BakkesPluginsLink(object? sender, RoutedEventArgs e)
    {
        Process.Start("explorer", @"https://bakkesplugins.com/plugins/view/53");
    }

    private void Button_InstallPluginURI(object? sender, RoutedEventArgs e)
    {
        Process.Start("explorer", @"bakkesmod://install/53");
    }

    private void preview_team_SelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        (_profileManager.Config.Event._game_state as GameState_RocketLeague).Player.Team = (int)((preview_team.SelectedItem as dynamic).Value);
    }

    private void preview_status_SelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        (_profileManager.Config.Event._game_state as GameState_RocketLeague).Game.Status = (RLStatus)(preview_status.SelectedItem);
    }

    private void preview_boost_amount_ValueChanged(object? sender, RoutedPropertyChangedEventArgs<double> e)
    {
        if (sender is not Slider slider) return;
        preview_boost_amount_label.Text = (int)(slider.Value * 100) + "%";

        if (!IsLoaded) return;
        (_profileManager.Config.Event._game_state as GameState_RocketLeague).Player.Boost = (float)(slider.Value);
    }

    private void preview_team1_score_ValueChanged(object? sender, RoutedPropertyChangedEventArgs<object> e)
    {
        if (sender is not IntegerUpDown { Value: not null } upDown) return;
        (_profileManager.Config.Event._game_state as GameState_RocketLeague).Match.Blue.Goals = upDown.Value ?? 0;
    }

    private void preview_team2_score_ValueChanged(object? sender, RoutedPropertyChangedEventArgs<object> e)
    {
        if (sender is IntegerUpDown upDown && upDown.Value.HasValue)
            (_profileManager.Config.Event._game_state as GameState_RocketLeague).Match.Orange.Goals = upDown.Value ?? 0;
    }

    private void ColorPicker_Team1_SelectedColorChanged(object? sender, RoutedPropertyChangedEventArgs<Color?> e)
    {
        if (sender is not ColorPicker) return;
        var clr = ColorPicker_team1.SelectedColor ?? new Color();
        (_profileManager.Config.Event._game_state as GameState_RocketLeague).Match.Blue.TeamColor = System.Drawing.Color.FromArgb(clr.A, clr.R, clr.G, clr.B);
    }

    private void ColorPicker_Team2_SelectedColorChanged(object? sender, RoutedPropertyChangedEventArgs<Color?> e)
    {
        if (sender is not ColorPicker) return;
        var clr = ColorPicker_team2.SelectedColor ?? new Color();
        (_profileManager.Config.Event._game_state as GameState_RocketLeague).Match.Orange.TeamColor = System.Drawing.Color.FromArgb(clr.A, clr.R, clr.G, clr.B);
    }
}