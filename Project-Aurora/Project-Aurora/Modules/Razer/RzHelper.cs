using System;
using Microsoft.Win32;
using RazerSdkReader;
using RazerSdkReader.Structures;

namespace AuroraRgb.Modules.Razer;

public class ChromaAppChangedEventArgs(string? applicationProcess) : EventArgs
{
    public string? ApplicationProcess { get; } = applicationProcess;
}

public static class RzHelper
{
    public static IRzGrid KeyboardColors { get; } = new ConnectedGrid();
    public static IRzGrid MousepadColors { get; } = new ConnectedGrid();
    public static IRzGrid MouseColors { get; } = new ConnectedGrid();
    public static IRzGrid HeadsetColors { get; } = new ConnectedGrid();
    public static IRzGrid ChromaLinkColors { get; } = new ConnectedGrid();

    public static event EventHandler<ChromaAppChangedEventArgs>? ChromaAppChanged;

    public static uint CurrentAppId { get; private set; }

    public static string? CurrentAppExecutable
    {
        get => _currentAppExecutable;
        private set
        {
            _currentAppExecutable = value;
            ChromaAppChanged?.Invoke(null, new ChromaAppChangedEventArgs(value));
        }
    }

    private static DateTime _lastFetch = DateTime.UnixEpoch;
    private static DateTime _lastUpdate = DateTime.UtcNow;
    private static string? _currentAppExecutable = string.Empty;

    /// <summary>
    /// Lowest supported version (inclusive)
    /// </summary>
    public static RzSdkVersion SupportedFromVersion => new(3, 12, 0);

    /// <summary>
    /// Highest supported version (exclusive)
    /// </summary>
    public static RzSdkVersion SupportedToVersion => new(4, 0, 0);

    public static RzSdkVersion GetSdkVersion()
    {
        try
        {
            using var hklm = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry32);
            var key = hklm.OpenSubKey(@"Software\Razer Chroma SDK");
            if (key is null)
            {
                return new RzSdkVersion(0, 0, 0);
            }

            var major = (int)key.GetValue("MajorVersion", 0);
            var minor = (int)key.GetValue("MinorVersion", 0);
            var revision = (int)key.GetValue("RevisionNumber", 0);
            key.Close();

            return new RzSdkVersion(major, minor, revision);
        }
        catch
        {
            // NOOP
        }

        return new RzSdkVersion(0, 0, 0);
    }

    public static bool IsSdkVersionSupported(RzSdkVersion version)
        => version >= SupportedFromVersion && version < SupportedToVersion;

    public static bool IsStale()
    {
        if (_lastFetch > _lastUpdate)
        {
            return true;
        }
        _lastFetch = DateTime.UtcNow;
        return false;
    }

    public static bool IsCurrentAppValid() => !string.IsNullOrEmpty(CurrentAppExecutable) && CurrentAppExecutable != Global.AuroraExe;

    public static void Initialize(ChromaReader sdkManager)
    {
        sdkManager.KeyboardUpdated += (object? _, in ChromaKeyboard keyboard) =>
        {
            KeyboardColors.Provider = keyboard;
            KeyboardColors.IsDirty = true;
            _lastUpdate = DateTime.UtcNow;
        };

        sdkManager.MouseUpdated += (object? _, in ChromaMouse mouse) =>
        {
            MouseColors.Provider = mouse;
            MouseColors.IsDirty = true;
            _lastUpdate = DateTime.UtcNow;
        };

        sdkManager.MousepadUpdated += (object? _, in ChromaMousepad mousepad) =>
        {
            MousepadColors.Provider = mousepad;
            MousepadColors.IsDirty = true;
            _lastUpdate = DateTime.UtcNow;
        };
        
        sdkManager.HeadsetUpdated += (object? _, in ChromaHeadset headset) =>
        {
            HeadsetColors.Provider = headset;
            HeadsetColors.IsDirty = true;
            _lastUpdate = DateTime.UtcNow;
        };

        sdkManager.ChromaLinkUpdated += (object? _, in ChromaLink link) =>
        {
            ChromaLinkColors.Provider = link;
            ChromaLinkColors.IsDirty = true;
            _lastUpdate = DateTime.UtcNow;
        };

        sdkManager.AppDataUpdated += (object? _, in ChromaAppData appData) =>
        {
            uint currentAppId = 0;
            string? currentAppName = null;
            for (var i = 0; i < appData.AppCount; i++)
            {
                if (appData.CurrentAppId != appData.AppInfo[i].AppId) continue;

                currentAppId = appData.CurrentAppId;
                currentAppName = appData.AppInfo[i].AppName;
                break;
            }

            CurrentAppId = currentAppId;
            CurrentAppExecutable = currentAppName;
        };
    }
}