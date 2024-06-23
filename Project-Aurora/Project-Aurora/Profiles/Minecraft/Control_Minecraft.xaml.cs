using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Net.Http;
using System.Windows;
using System.Windows.Controls;
using AuroraRgb.Profiles.Minecraft.GSI;
using Newtonsoft.Json.Linq;

namespace AuroraRgb.Profiles.Minecraft;

/// <summary>
/// Interaction logic for Control_Minecraft.xaml
/// </summary>
public partial class Control_Minecraft
{

    private readonly Application _profile;
    private readonly ObservableCollection<ModDetails> _modList = [];

    public Control_Minecraft(Application profile) {
        _profile = profile;

        InitializeComponent();
            
        ModListControl.ItemsSource = _modList;
        PopulateModList();
    }

    #region Overview handlers

    private void GoToForgePage_Click(object? sender, RoutedEventArgs e) {
        Process.Start("explorer", @"https://files.minecraftforge.net/");
    }

    private void GoToModDownloadPage_Click(object? sender, RoutedEventArgs e) {
        Process.Start("explorer", ((sender as Button).DataContext as ModDetails).Link);
    }

    private void GoToFabricDownloadPage(object? sender, RoutedEventArgs e) {
        Process.Start("explorer", @"https://fabricmc.net/");
    }

    private async void PopulateModList() {
        _modList.Clear();
        try {
            var client = new HttpClient();

            // Fetch all the data for the 'Aurora GSI Minecraft' group
            var groupDataResponse = await client.GetStringAsync(@"https://gitlab.com/api/v4/groups/aurora-gsi-minecraft/");
            var groupData = JObject.Parse(groupDataResponse);

            // For each project that is in this group
            foreach (var project in groupData["projects"]) {
                // Get the project tags (which contains the releases)
                var projectTagsResponse = await client.GetStringAsync($"{project["_links"]["self"]}/repository/tags?order_by=updated&sort=desc");
                var projectTags = JArray.Parse(projectTagsResponse);

                var versionStr = projectTags[0]["name"].ToString();

                // Add an entry to the mod details list
                _modList.Add(new ModDetails {
                    Name = project["name"].ToString(), // Get the project name (includes MC version)
                    Version = versionStr, // Get the version string (e.g. "v0.1.2")
                    Link = $"{project["web_url"]}/releases/{versionStr}", // Generate a link to the download page (e.g. "https://gitlab.com/aurora-gsi-minecraft/mc1.7.10/tags/v0.1.2")
                    Date = DateTime.Parse(projectTags[0]["commit"]["created_at"].ToString()).ToShortDateString() // Show the date the latest version of the mod was released.
                });
            }
                
        } catch { }
    }

    private void RefreshModList_Click(object? sender, RoutedEventArgs e) {
        PopulateModList();
    }
    #endregion

    #region Preview Handlers
    private GameStateMinecraft State => _profile.Config.Event.GameState as GameStateMinecraft;

    private void InGameCh_Checked(object? sender, RoutedEventArgs e) {
        State.Player.InGame = (sender as CheckBox).IsChecked ?? false;
    }

    private void HealthSlider_ValueChanged(object? sender, RoutedPropertyChangedEventArgs<double> e) {
        State.Player.Health = (float)e.NewValue;
        State.Player.HealthMax = 20f;
    }

    private void Slider_ValueChanged(object? sender, RoutedPropertyChangedEventArgs<double> e) {
        State.Player.Absorption = (float)e.NewValue;
    }

    private void HungerSlider_ValueChanged(object? sender, RoutedPropertyChangedEventArgs<double> e) {
        State.Player.FoodLevel = (int)e.NewValue;
    }

    private void ArmorSlider_ValueChanged(object? sender, RoutedPropertyChangedEventArgs<double> e) {
        State.Player.Armor = (int)e.NewValue;
    }

    private void ExperienceSlider_ValueChanged(object? sender, RoutedPropertyChangedEventArgs<double> e) {
        State.Player.Experience = (float)e.NewValue;
    }

    private void IsBurningCh_Checked(object? sender, RoutedEventArgs e) {
        State.Player.IsBurning = (sender as CheckBox).IsChecked ?? false;
    }

    private void IsInWaterCh_Checked(object? sender, RoutedEventArgs e) {
        State.Player.IsInWater = (sender as CheckBox).IsChecked ?? false;
    }

    private void IsSneakingCh_Checked(object? sender, RoutedEventArgs e) {
        State.Player.IsSneaking = (sender as CheckBox).IsChecked ?? false;
    }

    private void IsRidingCh_Checked(object? sender, RoutedEventArgs e) {
        State.Player.IsRidingHorse = (sender as CheckBox).IsChecked ?? false;
    }

    private void HasWitherCh_Checked(object? sender, RoutedEventArgs e) {
        State.Player.PlayerEffects.HasWither = (sender as CheckBox).IsChecked ?? false;
    }

    private void HasPoisonCh_Checked(object? sender, RoutedEventArgs e) {
        State.Player.PlayerEffects.HasPoison = (sender as CheckBox).IsChecked ?? false;
    }

    private void HasRegenCh_Checked(object? sender, RoutedEventArgs e) {
        State.Player.PlayerEffects.HasRegeneration = (sender as CheckBox).IsChecked ?? false;
    }

    private void RainStrengthSlider_ValueChanged(object? sender, RoutedPropertyChangedEventArgs<double> e) {
        State.World.RainStrength = (float)e.NewValue;
        State.World.IsRaining = e.NewValue > 0;
    }

    private void WorldTimeSlider_ValueChanged(object? sender, RoutedPropertyChangedEventArgs<double> e) {
        State.World.WorldTime = (long)(e.NewValue * 24000); // 24000 is max time in Minecraft before next day
        State.World.IsDayTime = e.NewValue <= 0.5; // At half point, it becomes night time?
    }
    #endregion
}

class ModDetails {
    public string Name { get; set; }
    public string Version { get; set; }
    public string Link { get; set; }
    public string Date { get; set; }
}