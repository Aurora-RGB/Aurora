using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Threading;
using AuroraRgb.Modules.Gamebar;
using Microsoft.Scripting.Utils;

namespace AuroraRgb.Profiles.GenericGames;

public partial class Control_GenericGames : INotifyPropertyChanged
{
    public ObservableCollection<string> EnabledPrograms { get; } = [];
    public ObservableCollection<string> ExcludedPrograms { get; } = [];
    public string SelectedEnabledProgram { get; set; } = "";
    public string SelectedExcludedProgram { get; set; } = "";
    
    private readonly GamebarGamesList _gamebarGames = GamebarGamesModule.GamebarGames;

    public Control_GenericGames(Application _)
    {
        InitializeComponent();
        RefreshEnabledPrograms();

        _gamebarGames.GameListChanged += ProfileOnGamesChanged;
        ExcludedPrograms.CollectionChanged += ExcludedProgramsOnCollectionChanged;
    }

    private void Control_Chroma_OnUnloaded(object sender, RoutedEventArgs e)
    {
        _gamebarGames.GameListChanged -= ProfileOnGamesChanged;
    }

    private void ProfileOnGamesChanged(object? sender, EventArgs e)
    {
        RefreshEnabledPrograms();
    }

    private async void ExcludedProgramsOnCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        if (GamebarGamesModule.GamebarConfigManager != null)
        {
            await GamebarGamesModule.GamebarConfigManager.SetExcludedPrograms(ExcludedPrograms.ToList());
        }
    }

    private void RefreshEnabledPrograms()
    {
        Dispatcher.Invoke(() =>
        {
            var excludedPrograms = GamebarGamesModule.GamebarConfigManager?.GetExcludedPrograms() ?? [];
            var enabledApps = GamebarGamesList.GetGameExes()
                .Except(excludedPrograms);

            EnabledPrograms.Clear();
            EnabledPrograms.AddRange(enabledApps);

            ExcludedPrograms.Clear();
            ExcludedPrograms.AddRange(excludedPrograms);
        }, DispatcherPriority.Input);
    }

    private void ExcludedAdd_Click(object? sender, RoutedEventArgs e)
    {
        var selection = SelectedEnabledProgram;
        if (string.IsNullOrEmpty(selection)) return;

        _gamebarGames.GameListChanged -= ProfileOnGamesChanged;
        ExcludedPrograms.Add(selection);
        EnabledPrograms.Remove(selection);
        _gamebarGames.GameListChanged += ProfileOnGamesChanged;
    }

    private void ExcludedRemove_Click(object? sender, RoutedEventArgs e)
    {
        var selection = SelectedExcludedProgram;
        if (string.IsNullOrEmpty(selection)) return;

        _gamebarGames.GameListChanged -= ProfileOnGamesChanged;
        EnabledPrograms.Add(selection);
        ExcludedPrograms.Remove(selection);
        _gamebarGames.GameListChanged += ProfileOnGamesChanged;
    }
}