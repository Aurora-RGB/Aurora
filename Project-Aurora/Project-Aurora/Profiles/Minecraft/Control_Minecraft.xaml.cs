using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using AuroraRgb.Profiles.Minecraft.GSI;

namespace AuroraRgb.Profiles.Minecraft;

/// <summary>
/// Interaction logic for Control_Minecraft.xaml
/// </summary>
public partial class Control_Minecraft
{
    private readonly Application _profile;
    private readonly ObservableCollection<ModDetails> _modList = [];

    public Control_Minecraft(Application profile)
    {
        _profile = profile;

        InitializeComponent();

        ModListControl.ItemsSource = _modList;
    }

    #region Overview handlers

    private void GoToForgePage_Click(object? sender, RoutedEventArgs e)
    {
        Process.Start("explorer", @"https://files.minecraftforge.net/");
    }

    private void GoToModDownloadPage_Click(object? sender, RoutedEventArgs e)
    {
        Process.Start("explorer", ((sender as Button).DataContext as ModDetails).Link);
    }

    private void GoToFabricDownloadPage(object? sender, RoutedEventArgs e)
    {
        Process.Start("explorer", @"https://fabricmc.net/");
    }

    private async Task PopulateModList()
    {
        _modList.Clear();
        try
        {
            var mcPlugins = await MinecraftPlugins.GetPlugins();

            var pluginVersion = mcPlugins.Version;

            foreach (var plugin in mcPlugins.Plugins)
            {
                var pluginName = plugin.Name;
                var pluginLink = plugin.DownloadUrl;
                var pluginDate = plugin.AssetUpdatedAt;

                // Add an entry to the mod details list
                _modList.Add(new ModDetails(
                    name: pluginName, // Get the project name (includes MC version)
                    version: pluginVersion, // Get the version string (e.g. "v0.1.2")
                    link: pluginLink, // Generate a link to the download page (e.g. "https://gitlab.com/aurora-gsi-minecraft/mc1.7.10/tags/v0.1.2")
                    date: pluginDate.Date.ToShortDateString() // Show the date the latest version of the mod was released.
                ));
            }
        }
        catch (Exception ex)
        {
            Global.logger.Error(ex, "Error fetching Minecraft plugins");
        }
    }

    private async void RefreshModList_Click(object? sender, RoutedEventArgs e)
    {
        await PopulateModList();
    }

    #endregion

    #region Preview Handlers

    private GameStateMinecraft State => _profile.Config.Event.GameState as GameStateMinecraft;

    private void InGameCh_Checked(object? sender, RoutedEventArgs e)
    {
        State.Player.InGame = (sender as CheckBox).IsChecked ?? false;
    }

    private void HealthSlider_ValueChanged(object? sender, RoutedPropertyChangedEventArgs<double> e)
    {
        State.Player.Health = (float)e.NewValue;
        State.Player.HealthMax = 20f;
    }

    private void Slider_ValueChanged(object? sender, RoutedPropertyChangedEventArgs<double> e)
    {
        State.Player.Absorption = (float)e.NewValue;
    }

    private void HungerSlider_ValueChanged(object? sender, RoutedPropertyChangedEventArgs<double> e)
    {
        State.Player.FoodLevel = (int)e.NewValue;
    }

    private void ArmorSlider_ValueChanged(object? sender, RoutedPropertyChangedEventArgs<double> e)
    {
        State.Player.Armor = (int)e.NewValue;
    }

    private void ExperienceSlider_ValueChanged(object? sender, RoutedPropertyChangedEventArgs<double> e)
    {
        State.Player.Experience = (float)e.NewValue;
    }

    private void IsBurningCh_Checked(object? sender, RoutedEventArgs e)
    {
        State.Player.IsBurning = (sender as CheckBox).IsChecked ?? false;
    }

    private void IsInWaterCh_Checked(object? sender, RoutedEventArgs e)
    {
        State.Player.IsInWater = (sender as CheckBox).IsChecked ?? false;
    }

    private void IsSneakingCh_Checked(object? sender, RoutedEventArgs e)
    {
        State.Player.IsSneaking = (sender as CheckBox).IsChecked ?? false;
    }

    private void IsRidingCh_Checked(object? sender, RoutedEventArgs e)
    {
        State.Player.IsRidingHorse = (sender as CheckBox).IsChecked ?? false;
    }

    private void HasWitherCh_Checked(object? sender, RoutedEventArgs e)
    {
        State.Player.MinecraftPlayerEffects.HasWither = (sender as CheckBox).IsChecked ?? false;
    }

    private void HasPoisonCh_Checked(object? sender, RoutedEventArgs e)
    {
        State.Player.MinecraftPlayerEffects.HasPoison = (sender as CheckBox).IsChecked ?? false;
    }

    private void HasRegenCh_Checked(object? sender, RoutedEventArgs e)
    {
        State.Player.MinecraftPlayerEffects.HasRegeneration = (sender as CheckBox).IsChecked ?? false;
    }

    private void RainStrengthSlider_ValueChanged(object? sender, RoutedPropertyChangedEventArgs<double> e)
    {
        State.World.RainStrength = (float)e.NewValue;
        State.World.IsRaining = e.NewValue > 0;
    }

    private void WorldTimeSlider_ValueChanged(object? sender, RoutedPropertyChangedEventArgs<double> e)
    {
        State.World.WorldTime = (long)(e.NewValue * 24000); // 24000 is max time in Minecraft before next day
        State.World.IsDayTime = e.NewValue <= 0.5; // At half point, it becomes night time?
    }

    #endregion

    private async void Control_Minecraft_OnLoaded(object sender, RoutedEventArgs e)
    {
        await PopulateModList();
    }
}

class ModDetails(string name, string version, string link, string date)
{
    public string Name { get; } = name;
    public string Version { get; } = version;
    public string Link { get; } = link;
    public string Date { get; } = date;
}