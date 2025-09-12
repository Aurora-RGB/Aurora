using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using AuroraRgb.Modules;
using AuroraRgb.Settings;

namespace AuroraRgb.Profiles.Logitech;

public partial class Control_Logitech : INotifyPropertyChanged
{
    public ObservableCollection<string> ExcludedPrograms { get; } = new(LogitechSdkModule.LightsyncConfig.DisabledApps);
    public string SelectedExcludedProgram { get; set; } = "";

    public Control_Logitech(Application application)
    {
        InitializeComponent();
        
        ExcludedPrograms.CollectionChanged += ExcludedProgramsOnCollectionChanged;
    }

    private void ExcludedProgramsOnCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        LogitechSdkModule.LightsyncConfig.DisabledApps = ExcludedPrograms.ToList();
        ConfigManager.Save(LogitechSdkModule.LightsyncConfig);
    }

    private void ExcludeAdd_Click(object sender, RoutedEventArgs e)
    {
        var excludedProgram = ExcludeProcessTb.Text;
        if (string.IsNullOrWhiteSpace(excludedProgram) || ExcludedPrograms.Contains(excludedProgram)) return;

        ExcludedPrograms.Add(excludedProgram);
        ExcludeProcessTb.Clear();
    }

    private void ExcludedRemove_Click(object sender, RoutedEventArgs e)
    {
        if (string.IsNullOrWhiteSpace(SelectedExcludedProgram) || !ExcludedPrograms.Contains(SelectedExcludedProgram))
            return;

        ExcludedPrograms.Remove(SelectedExcludedProgram);
        SelectedExcludedProgram = "";
    }
}