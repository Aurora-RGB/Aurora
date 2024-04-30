using System.Diagnostics;
using System.Windows;

namespace AuroraRgb.Profiles.StardewValley;

public partial class Control_StardewValley
{
    private Application _profile;

    public Control_StardewValley(Application profile)
    {
        _profile = profile;

        InitializeComponent();
    }

    private void GoToSMAPIPage_Click(object? sender, RoutedEventArgs e)
    {
        Process.Start("explorer", @"https://www.nexusmods.com/stardewvalley/mods/2400");
    }

    private void GoToModDownloadPage_Click(object? sender, RoutedEventArgs e)
    {
        Process.Start("explorer", @"https://www.nexusmods.com/stardewvalley/mods/6088");
    }
}