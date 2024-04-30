using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Threading;
using AuroraRgb.Devices;
using Microsoft.Scripting.Utils;
using Microsoft.Win32;

namespace AuroraRgb.Profiles.Chroma;

public partial class Control_Chroma : INotifyPropertyChanged
{
    private static readonly HashSet<string> ExcludedApps = [Global.AuroraExe, DeviceManager.DeviceManagerExe];
    
    public ObservableCollection<string> EnabledPrograms { get; } = [];
    public ObservableCollection<string> ExcludedPrograms => _settings.ExcludedPrograms;
    public string SelectedEnabledProgram { get; set; } = "";
    public string SelectedExcludedProgram { get; set; } = "";

    private readonly ChromaApplication _profile;
    private readonly ChromaApplicationSettings _settings;

    public Control_Chroma(Application profile)
    {
        _profile = (ChromaApplication)profile;
        _settings = (ChromaApplicationSettings)profile.Settings;

        InitializeComponent();

        _settings.ExcludedPrograms.CollectionChanged += ExcludedProgramsOnCollectionChanged;
        _profile.ChromaAppsChanged += ProfileOnChromaAppsChanged;

        RefreshEnabledPrograms();
        ReorderChromaRegistry();
    }

    private void ProfileOnChromaAppsChanged(object? sender, EventArgs e)
    {
        RefreshEnabledPrograms();
    }

    private void ExcludedProgramsOnCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        RefreshEnabledPrograms();
    }

    private void RefreshEnabledPrograms()
    {
        Dispatcher.Invoke(() =>
        {
            EnabledPrograms.Clear();
            EnabledPrograms.AddRange(_profile.AllChromaApps.Except(_settings.ExcludedPrograms));
        }, DispatcherPriority.Input);
    }

    private void ExcludedAdd_Click(object? sender, RoutedEventArgs e)
    {
        if (string.IsNullOrEmpty(SelectedEnabledProgram)) return;
        _settings.ExcludedPrograms.Add(SelectedEnabledProgram);
        ReorderChromaRegistry();
    }

    private void ExcludedRemove_Click(object? sender, RoutedEventArgs e)
    {
        var exclusion = SelectedExcludedProgram;
        if (string.IsNullOrEmpty(exclusion)) return;
        _settings.ExcludedPrograms.Remove(exclusion);
        _profile.Config.ProcessNames = _profile.Config.ProcessNames
            .Append(exclusion.ToLower())
            .ToArray();
        ReorderChromaRegistry();
    }

    private void ReorderChromaRegistry()
    {
        var value = string.Join(';', EnabledPrograms.Where(NonEmpty).Concat(ExcludedApps));
        if (ExcludedPrograms.Count > 0)
        {
            value += ";";
            value += string.Join(';', ExcludedPrograms.Where(NonEmpty).Where(s => !ExcludedApps.Contains(s)));
        }
        
        using var registryKey = Registry.LocalMachine.OpenSubKey(ChromaApplication.AppsKey, true);
        registryKey?.SetValue(ChromaApplication.PriorityValue, value);
    }

    private bool NonEmpty(string s)
    {
        return !string.IsNullOrWhiteSpace(s);
    }
}