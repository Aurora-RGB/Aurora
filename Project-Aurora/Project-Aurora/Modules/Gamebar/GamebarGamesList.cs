using System;
using System.Collections.Generic;
using System.IO;
using System.Management;
using System.Security.Principal;
using Microsoft.Win32;

namespace AuroraRgb.Modules.Gamebar;

public sealed class GamebarGamesList : IDisposable
{
    public event EventHandler? GameListChanged;
    public List<string> GameExes { get; } = new(25);

    private const string GamebarConfigStoreKey = @"System\GameConfigStore\Children";

    private ManagementEventWatcher? _eventWatcher;
    private readonly WqlEventQuery _query;
    private readonly ManagementScope _scope;

    public GamebarGamesList()
    {
        var currentUser = WindowsIdentity.GetCurrent().User!.Value;
        _scope = new ManagementScope(@"\\.\root\default");

        var key = GamebarConfigStoreKey.Replace(@"\", @"\\\\");

        var queryString =
            $"""
             SELECT * FROM RegistryKeyChangeEvent 
                      WHERE Hive='HKEY_USERS'
                        AND KeyPath = '{currentUser}\\{key}'
             """;
        _query = new WqlEventQuery(queryString);
    }

    private static IEnumerable<string> GetGameExes()
    {
        using var gamebarProfiles = Registry.CurrentUser.OpenSubKey(GamebarConfigStoreKey);
        if (gamebarProfiles == null)
            yield break;

        foreach (var subKeyName in gamebarProfiles.GetSubKeyNames())
        {
            using var subKey = gamebarProfiles.OpenSubKey(subKeyName);

            if (subKey?.GetValue("MatchedExeFullPath") is string matchedExeFullPath)
            {
                // get file name from path
                yield return Path.GetFileName(matchedExeFullPath);
            }
        }
    }

    public void StartWatching()
    {
        _eventWatcher = new ManagementEventWatcher(_scope, _query);
        _eventWatcher.EventArrived += KeyWatcherOnEventArrived;
        try
        {
            _eventWatcher.Start();
        }
        catch (Exception e)
        {
            Global.logger.Error(e, "Error starting Gamebar profiles watcher");
        }
    }

    public void StopWatching()
    {
        if (_eventWatcher == null)
        {
            return;
        }

        _eventWatcher.EventArrived -= KeyWatcherOnEventArrived;
        _eventWatcher.Stop();
        _eventWatcher.Dispose();
        _eventWatcher = null;
    }

    private void KeyWatcherOnEventArrived(object? sender, EventArrivedEventArgs e)
    {
        GameExes.Clear();
        GameExes.AddRange(GetGameExes());
        GameListChanged?.Invoke(this, EventArgs.Empty);
    }

    public void Dispose()
    {
        StopWatching();
    }
}