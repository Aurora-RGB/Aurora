using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Threading;
using AuroraRgb.Modules;
using AuroraRgb.Modules.Razer;
using Microsoft.Scripting.Utils;

namespace AuroraRgb.Profiles.Chroma;

public partial class Control_Chroma : INotifyPropertyChanged
{
    public ObservableCollection<string> EnabledPrograms { get; } = [];
    public ObservableCollection<string> ExcludedPrograms { get; } = [];
    public string SelectedEnabledProgram { get; set; } = "";
    public string SelectedExcludedProgram { get; set; } = "";

    public Control_Chroma(Application _)
    {
        InitializeComponent();
        RefreshEnabledPrograms();

        ChromaRegistrySettings().ChromaAppsChanged += ProfileOnChromaAppsChanged;
        ExcludedPrograms.CollectionChanged += ExcludedProgramsOnCollectionChanged;
    }

    private void Control_Chroma_OnUnloaded(object sender, RoutedEventArgs e)
    {
        ChromaRegistrySettings().ChromaAppsChanged -= ProfileOnChromaAppsChanged;
    }

    private void ProfileOnChromaAppsChanged(object? sender, EventArgs e)
    {
        RefreshEnabledPrograms();
    }

    private void ExcludedProgramsOnCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        ChromaRegistrySettings().SetExcludedPrograms(ExcludedPrograms.ToList());
    }

    private void RefreshEnabledPrograms()
    {
        Dispatcher.Invoke(() =>
        {
            var enabledApps = ChromaRegistrySettings().AllChromaApps
                .Except(ChromaRegistrySettings().ExcludedPrograms);
            var disabledApps = ChromaRegistrySettings().ExcludedPrograms;

            EnabledPrograms.Clear();
            EnabledPrograms.AddRange(enabledApps);

            ExcludedPrograms.Clear();
            ExcludedPrograms.AddRange(disabledApps);
        }, DispatcherPriority.Input);
    }

    private void ExcludedAdd_Click(object? sender, RoutedEventArgs e)
    {
        var selection = SelectedEnabledProgram;
        if (string.IsNullOrEmpty(selection)) return;

        ChromaRegistrySettings().ChromaAppsChanged -= ProfileOnChromaAppsChanged;
        ExcludedPrograms.Add(selection);
        EnabledPrograms.Remove(selection);
        ChromaRegistrySettings().ChromaAppsChanged += ProfileOnChromaAppsChanged;
    }

    private void ExcludedRemove_Click(object? sender, RoutedEventArgs e)
    {
        var selection = SelectedExcludedProgram;
        if (string.IsNullOrEmpty(selection)) return;

        ChromaRegistrySettings().ChromaAppsChanged -= ProfileOnChromaAppsChanged;
        EnabledPrograms.Add(selection);
        ExcludedPrograms.Remove(selection);
        ChromaRegistrySettings().ChromaAppsChanged += ProfileOnChromaAppsChanged;
    }

    private static ChromaRegistrySettings ChromaRegistrySettings()
    {
        return RazerSdkModule.RzSdkManager.Result.ChromaRegistrySettings;
    }
}