using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using AuroraRgb.Profiles.Subnautica.GSI;
using Xceed.Wpf.Toolkit;

namespace AuroraRgb.Profiles.Subnautica;

/// <summary>
/// Interaction logic for Control_Subnautica.xaml
/// </summary>
public partial class Control_Subnautica
{
    private readonly Application _profile;

    public Control_Subnautica(Application profile)
    {
        _profile = profile;

        InitializeComponent();
    }

    #region Overview handlers

    private void GoToQModManagerPage_Click(object? sender, RoutedEventArgs e)
    {
        Process.Start("explorer", @"https://www.nexusmods.com/subnautica/mods/16/");
    }

    private void GoToModDownloadPage_Click(object? sender, RoutedEventArgs e)
    {
        Process.Start("explorer", @"https://www.nexusmods.com/subnautica/mods/171");
    }

    #endregion
        
    #region Preview Handlers
    private GameStateSubnautica State => _profile.Config.Event.GameState as GameStateSubnautica;

    private void InGameCh_Checked(object? sender, RoutedEventArgs e)
    {
        if ((sender as CheckBox).IsChecked == true)
        {
            State.GameState.GameState = 2;
            State.GameState.InGame = true;
        }
        else
        {
            State.GameState.GameState = 0;
            State.GameState.InGame = false;
        }
    }

    private void HealthSlider_ValueChanged(object? sender, RoutedPropertyChangedEventArgs<double> e)
    {
        State.Player.Health = (int)e.NewValue;
    }

    private void HungerSlider_ValueChanged(object? sender, RoutedPropertyChangedEventArgs<double> e)
    {
        State.Player.Food = (int)e.NewValue;
    }

    private void WaterSlider_ValueChanged(object? sender, RoutedPropertyChangedEventArgs<double> e)
    {
        State.Player.Water = (int)e.NewValue;
    }

    private void Oxygen_ValueChanged(object? sender, RoutedPropertyChangedEventArgs<double> e)
    {
        State.Player.OxygenAvailable = (int)e.NewValue;
    }

    private void preview_DepthLevel_ValueChanged(object? sender, RoutedPropertyChangedEventArgs<object> e)
    {
        if (IsLoaded && sender is IntegerUpDown && (sender as IntegerUpDown).Value.HasValue)
            State.Player.DepthLevel = (sender as IntegerUpDown).Value.Value;
    }
    #endregion
}