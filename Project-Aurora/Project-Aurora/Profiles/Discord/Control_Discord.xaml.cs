using System;
using System.IO;
using System.Net.Http;
using System.Windows;
using System.Windows.Forms;
using MessageBox = System.Windows.MessageBox;

namespace AuroraRgb.Profiles.Discord;

/// <summary>
/// Interaction logic for Control_Minecraft.xaml
/// </summary>
public partial class Control_Discord
{
    public Control_Discord(Application _) {
        InitializeComponent();
    }

    private void PatchButton_Click(object? sender, RoutedEventArgs e)
    {
        var appdata = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        var pluginDirectory = Path.Combine(appdata, "BetterDiscord", "plugins");

        if (!Directory.Exists(pluginDirectory))
            Directory.CreateDirectory(pluginDirectory);

        var pluginFile = Path.Combine(pluginDirectory, "AuroraGSI.plugin.js");
        WriteFile(pluginFile);
    }

    private void UnpatchButton_Click(object? sender, RoutedEventArgs e)
    {
        var appdata = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        var path = Path.Combine(appdata, "BetterDiscord", "plugins", "AuroraGSI.plugin.js");

        if (File.Exists(path))
        {
            File.Delete(path);
            MessageBox.Show("Plugin uninstalled successfully");
            return;
        }

        MessageBox.Show("Plugin not found.");
    }

    private void ManualPatchButton_Click(object? sender, RoutedEventArgs e)
    {
        using var dialog = new FolderBrowserDialog();
        if (dialog.ShowDialog() != DialogResult.OK)
            return;

        var pluginFile = Path.Combine(dialog.SelectedPath, "AuroraGSI.plugin.js");
        WriteFile(pluginFile);
    }

    private async void WriteFile(string pluginFile)
    {
        try
        {
            const string url = "https://raw.githubusercontent.com/Aurora-RGB/Discord-GSI/master/AuroraGSI.plugin.js";
            using var httpClient = new HttpClient();

            var response = await httpClient.GetAsync(url);

            await using var contentStream = await response.Content.ReadAsStreamAsync();
            await using var fileStream = File.Create(pluginFile);

            await contentStream.CopyToAsync(fileStream);

            MessageBox.Show("Plugin installed successfully");
        }
        catch (Exception e)
        {
            MessageBox.Show("Error installing plugin: " + e.Message);
        }
    }
}