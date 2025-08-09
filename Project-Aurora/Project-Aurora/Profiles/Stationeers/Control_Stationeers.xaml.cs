using AuroraRgb.Profiles.Stationeers.GSI;
using AuroraRgb.Profiles.Stationeers.GSI.Nodes;
using AuroraRgb.Profiles.Subnautica.GSI;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using Xceed.Wpf.Toolkit;

namespace AuroraRgb.Profiles.Stationeers;

/// <summary>
/// Interaction logic for Control_Subnautica
public partial class Control_Stationeers
{
    private readonly Application _profile;

    public Control_Stationeers(Application profile)
    {
        _profile = profile;

        InitializeComponent();
    }

    #region Overview handlers

    private void GoBepinxExPage_Click(object? sender, RoutedEventArgs e)
    {
        Process.Start("explorer", @"https://github.com/BepInEx/BepInEx/releases");
    }

    private void GoSLPPage_Click(object? sender, RoutedEventArgs e)
    {
        Process.Start("explorer", @"https://github.com/StationeersLaunchPad/StationeersLaunchPad/releases");
    }

    private void GoToModDownloadPage_Click(object? sender, RoutedEventArgs e)
    {
        Process.Start("explorer", @"https://www.nexusmods.com/stationeers/mods/16");
    }
    #endregion

    #region Preview Handlers

    private GameStateStationeers State => _profile.Config.Event.GameState as GameStateStationeers;

    private void HealthSlider_ValueChanged(object? sender, RoutedPropertyChangedEventArgs<double> e)
    {
        if(State.Player != null)
        State.Player.Health = (int)e.NewValue;
    }

    private void HungerSlider_ValueChanged(object? sender, RoutedPropertyChangedEventArgs<double> e)
    {
        if (State.Player != null)
            State.Player.Food = (int)e.NewValue;
    }

    private void WaterSlider_ValueChanged(object? sender, RoutedPropertyChangedEventArgs<double> e)
    {
        if (State.Player != null)
            State.Player.Water = (int)e.NewValue;
    }

    private void Oxygen_ValueChanged(object? sender, RoutedPropertyChangedEventArgs<double> e)
    {
        if (State.Player != null)
        {
            State.Player.OxygenTankLevel = (int)e.NewValue;
            State.Player.OxygenTankCapacity = 100;
        }
    }

    private void Waste_ValueChanged(object? sender, RoutedPropertyChangedEventArgs<double> e)
    {
        if (State.Player != null)
        {
            State.Player.WasteTankLevel = (int)e.NewValue;
            State.Player.WasteTankCapacity = 100;
        }
    }

    private void Fuel_ValueChanged(object? sender, RoutedPropertyChangedEventArgs<double> e)
    {
        if (State.Player != null)
        {
            State.Player.FuelTankLevel = (int)e.NewValue;
            State.Player.FuelTankCapacity = 100;
        }
    }

    private void Temp_ValueChanged(object? sender, RoutedPropertyChangedEventArgs<double> e)
    {
        if (State.Player != null)
            State.Player.Temperature = (int)e.NewValue;
    }

    private void Pressure_ValueChanged(object? sender, RoutedPropertyChangedEventArgs<double> e)
    {
        if (State.Player != null)
            State.Player.Pressure = (int)e.NewValue;
    }


    private void Preview_BatteryLevel_ValueChanged(object? sender, RoutedPropertyChangedEventArgs<object> e)
    {
        if (State.Player != null)
            if (IsLoaded && sender is IntegerUpDown && (sender as IntegerUpDown).Value.HasValue)
            State.Player.Battery = (sender as IntegerUpDown).Value.Value;
    }
    #endregion
}