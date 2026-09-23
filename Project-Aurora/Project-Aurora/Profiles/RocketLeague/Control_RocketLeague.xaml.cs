using System;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using AuroraRgb.Profiles.RocketLeague.GSI;
using AuroraRgb.Utils.Steam;
using Xceed.Wpf.Toolkit;
using MessageBox = System.Windows.Forms.MessageBox;

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
            preview_status.ItemsSource = Enum.GetValues(typeof(RlStatus)).Cast<RlStatus>();
            preview_status.SelectedIndex = (int)RlStatus.InGame;
        }

        ColorPicker_team1.SelectedColor = Colors.Blue;
        ColorPicker_team2.SelectedColor = Colors.Orange;
        preview_team1_score.Value = 0;
        preview_team2_score.Value = 0;
    }

    private async void ButtonBase_OnClick(object sender, RoutedEventArgs e)
    {
        if (_profileManager.Config.Application?.Settings is not RlSettings rlSettings) return;
        var setPort = PortTextBox.Value ?? 49123;
        rlSettings.SocketPort = setPort;

        var gamePath = await SteamUtils.GetGamePathAsync(252950);
        if (string.IsNullOrEmpty(gamePath))        {
            MessageBox.Show("Rocket League is not installed via Steam or Steam installation could not be found.");
            return;
        }
        
        //<Install Dir>\TAGame\Config\DefaultStatsAPI.ini
        // set .ini PacketSendRate = 30
        var iniPath = Path.Join(gamePath, "TAGame", "Config", "DefaultStatsAPI.ini");
        if (!File.Exists(iniPath))
        {
            MessageBox.Show("Rocket League's DefaultStatsAPI.ini file could not be found.");
            return;
        }

        // set rocket league .ini
        await RlStatsInstallUtils.EnableRlSocket(iniPath, setPort);
    }

    private void preview_team_SelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        if (_profileManager.Config.Event.GameState is not GameStateRocketLeague gameStateRocketLeague) return;
        gameStateRocketLeague.Player?.TeamNum = (int)(preview_team.SelectedItem as dynamic).Value;
    }

    private void preview_status_SelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        if (_profileManager.Config.Event.GameState is not GameStateRocketLeague gameStateRocketLeague) return;
        gameStateRocketLeague.GameStatus = (RlStatus)(preview_status.SelectedItem);
    }

    private void preview_boost_amount_ValueChanged(object? sender, RoutedPropertyChangedEventArgs<double> e)
    {
        if (sender is not Slider slider) return;
        preview_boost_amount_label.Text = (int)(slider.Value * 100) + "%";

        if (!IsLoaded) return;
        if (_profileManager.Config.Event.GameState is not GameStateRocketLeague gameStateRocketLeague) return;
        gameStateRocketLeague.Player?.Boost = (int)Math.Round(slider.Value);
    }

    private void preview_team1_score_ValueChanged(object? sender, RoutedPropertyChangedEventArgs<object> e)
    {
        if (sender is not IntegerUpDown { Value: not null } upDown) return;
        if (_profileManager.Config.Event.GameState is GameStateRocketLeague gameStateRocketLeague)
            gameStateRocketLeague.Game.Blue?.Goals = upDown.Value ?? 0;
    }

    private void preview_team2_score_ValueChanged(object? sender, RoutedPropertyChangedEventArgs<object> e)
    {
        if (sender is not IntegerUpDown { Value: not null } upDown) return;
        if (_profileManager.Config.Event.GameState is GameStateRocketLeague gameStateRocketLeague)
            gameStateRocketLeague.Game.Orange?.Goals = upDown.Value ?? 0;
    }

    private void ColorPicker_Team1_SelectedColorChanged(object? sender, RoutedPropertyChangedEventArgs<Color?> e)
    {
        if (sender is not ColorPicker) return;
        var clr = ColorPicker_team1.SelectedColor ?? new Color();
        if (_profileManager.Config.Event.GameState is GameStateRocketLeague gameStateRocketLeague)
            gameStateRocketLeague.Game.Blue?.ColorPrimary = System.Drawing.Color.FromArgb(clr.A, clr.R, clr.G, clr.B);
    }

    private void ColorPicker_Team2_SelectedColorChanged(object? sender, RoutedPropertyChangedEventArgs<Color?> e)
    {
        if (sender is not ColorPicker) return;
        var clr = ColorPicker_team2.SelectedColor ?? new Color();
        if (_profileManager.Config.Event.GameState is GameStateRocketLeague gameStateRocketLeague)
            gameStateRocketLeague.Game.Orange?.ColorPrimary = System.Drawing.Color.FromArgb(clr.A, clr.R, clr.G, clr.B);
    }

    private void Control_RocketLeague_OnLoaded(object sender, RoutedEventArgs e)
    {
        if (_profileManager.Config.Application?.Settings is RlSettings rlSettings)
        {
            PortTextBox.Value = rlSettings.SocketPort;
        }
    }
}