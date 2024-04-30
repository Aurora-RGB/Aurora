using System;
using System.IO;
using System.Windows;
using System.Windows.Forms;
using AuroraRgb.Utils;
using Microsoft.Win32;
using MessageBox = System.Windows.MessageBox;

namespace AuroraRgb.Profiles.LeagueOfLegends;

/// <summary>
/// Interaction logic for Control_LoL.xaml
/// </summary>
public partial class Control_LoL
{
    public Control_LoL(Application _)
    {
        InitializeComponent();
    }

    private void Button_Click(object? sender, RoutedEventArgs e)
    {
        string lolpath;
        try
        {
            lolpath = (string)Registry.GetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\WOW6432Node\Riot Games, Inc\League of Legends", "Location", null);
        }
        catch
        {
            lolpath = String.Empty;
        }

        if (string.IsNullOrWhiteSpace(lolpath))
        {
            MessageBox.Show("Could not find the league of legends path automatically. Please select the correct location(Usually in c:\\Riot Games\\League of Legends)");
            var fp = new FolderBrowserDialog();
            if(fp.ShowDialog() != DialogResult.OK)
            {
                MessageBox.Show("Could not remove wrapper patch");
                return;
            }
            if(!fp.SelectedPath.EndsWith("League of Legends"))
            {
                MessageBox.Show("Could not remove wrapper patch");
                return;
            }
            lolpath = fp.SelectedPath;
        }

        if (FileUtils.TryDelete(Path.Combine(lolpath, "Game", "LightFx.dll")))
            MessageBox.Show("Deleted file successfully");
        else
            MessageBox.Show("Could not find the wrapper file.");
    }
}