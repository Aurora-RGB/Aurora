using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AuroraRgb.Settings;
using AuroraRgb.Utils;
using Debounce.Core;
using Microsoft.Win32;

namespace AuroraRgb.Modules.Razer;

public class ChromaRegistrySettings
{
    public event EventHandler<EventArgs>? ChromaAppsChanged;

    public List<string> AllChromaApps { get; private set; } = [];
    public List<string> ExcludedPrograms => _auroraChromaSettings.DisabledApps;

    private const string AppsKey = @"SOFTWARE\\WOW6432Node\\Razer Chroma SDK\\Apps";
    private const string PriorityValue = "PriorityList";

    private readonly RegistryWatcher _registryWatcher = new(RegistryHiveOpt.LocalMachine, AppsKey, PriorityValue);
    private readonly AuroraChromaSettings _auroraChromaSettings;

    private readonly Debouncer _settingsSaveDebouncer;

    public ChromaRegistrySettings(AuroraChromaSettings auroraChromaSettings)
    {
        _auroraChromaSettings = auroraChromaSettings;
        _settingsSaveDebouncer = new Debouncer(() =>
        {
            SaveSettings().Wait();
        }, 850);
    }

    public void Initialize()
    {
        _registryWatcher.RegistryChanged += RegistryWatcherOnRegistryChanged;
        _registryWatcher.StartWatching();
    }

    public void SetExcludedPrograms(List<string> excludedPrograms)
    {
        _auroraChromaSettings.DisabledApps = excludedPrograms.Where(NonEmpty).ToList();
        ReorderChromaRegistry();
        
        _settingsSaveDebouncer.Debounce();
        
        ChromaAppsChanged?.Invoke(this, EventArgs.Empty);
    }

    private async Task SaveSettings()
    {
        await ConfigManager.SaveAsync(_auroraChromaSettings);
    }

    private void RegistryWatcherOnRegistryChanged(object? sender, RegistryChangedEventArgs e)
    {
        if (e.Data is not string chromaAppList)
        {
            return;
        }

        AllChromaApps = chromaAppList.Split(';').ToList();
        FilterAndSetProcesses();
    }

    private void FilterAndSetProcesses()
    {
        ChromaAppsChanged?.Invoke(this, EventArgs.Empty);
    }

    private void ReorderChromaRegistry()
    {
        var value = string.Join(';', AllChromaApps.OrderBy(app => ExcludedPrograms.Contains(app)));
        
        using var registryKey = Registry.LocalMachine.OpenSubKey(AppsKey, true);
        registryKey?.SetValue(PriorityValue, value);
    }

    private static bool NonEmpty(string s)
    {
        return !string.IsNullOrWhiteSpace(s);
    }
}