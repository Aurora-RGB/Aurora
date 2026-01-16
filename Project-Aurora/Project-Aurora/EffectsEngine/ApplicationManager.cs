using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using AuroraRgb.Controls;
using AuroraRgb.Modules;
using AuroraRgb.Modules.GameStateListen;
using AuroraRgb.Modules.ProcessMonitor;
using AuroraRgb.Profiles;
using AuroraRgb.Profiles.Desktop;
using AuroraRgb.Profiles.Generic_Application;
using Common.Utils;
using AssemblyExtensions = AuroraRgb.Utils.AssemblyExtensions;

namespace AuroraRgb.EffectsEngine;

public sealed class ApplicationInitializedEventArgs(Application application) : EventArgs
{
    public Application Application { get; } = application;
}

public sealed class ApplicationManager : IAsyncDisposable, IDisposable
{
    public event EventHandler<ApplicationInitializedEventArgs>? EventAdded;
    public event EventHandler? OverlayProfilesChanged;

    public Dictionary<string, Application> Events { get; } = new() { { "desktop", new Desktop() } };

    public Desktop DesktopProfile => (Desktop)Events["desktop"];
    
    
    private string _previewModeProfileKey = "";

    public string? PreviewProfileKey {
        get => _previewModeProfileKey;
        set => _previewModeProfileKey = value ?? string.Empty;
    }

    private Dictionary<string, SortedSet<Application>> EventProcesses { get; } = new();
    private Dictionary<Regex, Application> EventTitles { get; } = new();
    private Dictionary<string, Application> EventAppIDs { get; } = new();
    public ImmutableHashSet<string> OverlayActiveProfiles { get; private set; } = [];

    private readonly CancellationTokenSource _initializeCancelSource = new();
    private readonly ConcurrentQueue<Func<Task>> _initTaskQueue = new();
    
    private readonly Task<RunningProcessMonitor> _runningProcessMonitor;
    private readonly Task<ActiveProcessMonitor> _activeProcessMonitor;

    private Func<Application, bool> _isOverlayActiveProfile = _ => false; // Initialized after _runningProcessMonitor is ready
    private readonly SingleConcurrentThread _profileInitThread;

    private bool Initialized { get; set; }

    public ApplicationManager(Task<RunningProcessMonitor> runningProcessMonitor, Task<ActiveProcessMonitor> activeProcessMonitor)
    {
        _runningProcessMonitor = runningProcessMonitor;
        _activeProcessMonitor = activeProcessMonitor;

        _profileInitThread = new SingleConcurrentThread("ProfileInit", ProfileInitAction, ProfileInitExceptionCallback);
    }

    public async Task Initialize()
    {
        if (Initialized)
            return;

        if (_initializeCancelSource.IsCancellationRequested)
            return;

        var cancellationToken = _initializeCancelSource.Token;
        var defaultApps = EnumerateDefaultApps();
        var userApps = EnumerateUserApps();

        // Register all Application types in the assembly
        var profiles = defaultApps.Concat(userApps);
        foreach (var inst in profiles)
            await RegisterEvent(inst);

        await DesktopProfile.Initialize(cancellationToken);
        LoadSettings();

        var runningProcessMonitor = await _runningProcessMonitor;
        Predicate<string> processRunning = ProcessRunning;
        _isOverlayActiveProfile = evt => evt.IsOverlayEnabled &&
                                         Array.Exists(evt.Config.ProcessNames, processRunning);

        // Listen for profile keybind triggers
        // TODO make this optional
        (await InputsModule.InputEvents).KeyDown += CheckProfileKeybinds;

        Initialized = true;

        bool ProcessRunning(string name) => runningProcessMonitor.IsProcessRunning(name);
    }

    private static IEnumerable<Application> EnumerateDefaultApps()
    {
        return from type in AssemblyExtensions.GetLoadableTypes(Assembly.GetExecutingAssembly())
            where (type.BaseType == typeof(Application) || type.BaseType == typeof(GsiApplication)) && type != typeof(GenericApplication) && type != typeof(GsiApplication)
            let inst = (Application)Activator.CreateInstance(type)
            orderby inst.Config.Name
            select inst;
    }

    private static IEnumerable<Application> EnumerateUserApps()
    {
        var additionalProfilesPath = Path.Combine(Global.AppDataDirectory, "AdditionalProfiles");
        if (!Directory.Exists(additionalProfilesPath))
        {
            return [];
        }

        var userApps = from dir in Directory.EnumerateDirectories(additionalProfilesPath)
            where File.Exists(Path.Combine(dir, "settings.json"))
            select Path.GetFileName(dir);

        return userApps.Select(processName => new GenericApplication(processName));
    }

    public void InitializeApps()
    {
        var cancellationToken = _initializeCancelSource.Token;
        foreach (var (_, profile) in Events)
        {
            _initTaskQueue.Enqueue(async () =>
            {
                try
                {
                    await profile.Initialize(cancellationToken);
                    EventAdded?.Invoke(this, new ApplicationInitializedEventArgs(profile));
                }
                catch (Exception e)
                {
                    Global.logger.Error(e, "Error initializing profile {Profile}", profile.GetType());
                }
            });
        }

        _profileInitThread.Trigger();
    }

    private void LoadSettings()
    {
        foreach (var kvp in Events
                     .Where(kvp => !Global.Configuration.ProfileOrder.Contains(kvp.Key) && kvp.Value is Application))
        {
            Global.Configuration.ProfileOrder.Add(kvp.Key);
        }

        Global.Configuration.ProfileOrder
            .RemoveAll(key => !Events.ContainsKey(key) || Events[key] is not Application);

        PutProfileTop("games");
        PutProfileTop("logitech");
        PutProfileTop("icue");
        PutProfileTop("chroma");
        PutProfileTop("desktop");
    }

    public async Task RegisterEvent(Application application)
    {
        var profileId = application.Config.ID;
        if (string.IsNullOrWhiteSpace(profileId) || !Events.TryAdd(profileId, application)) return;

        foreach (var exe in application.Config.ProcessNames)
        {
            AddEventProcess(exe, application);
        }

        AddEventProcess(profileId, application);

        application.Config.ProcessNamesChanged += ConfigOnProcessNamesChanged(application);

        if (application.Config.ProcessTitles != null)
            foreach (var titleRx in application.Config.ProcessTitles)
                EventTitles.Add(titleRx, application);

        if (!string.IsNullOrWhiteSpace(profileId))
            EventAppIDs.Add(profileId, application);

        if (!Global.Configuration.ProfileOrder.Contains(profileId))
        {
            Global.Configuration.ProfileOrder.Add(profileId);
        }

        if (Initialized)
            await application.Initialize(_initializeCancelSource.Token);
    }

    private EventHandler<EventArgs> ConfigOnProcessNamesChanged(Application application)
    {
        return (_, _) =>
        {
            var profileId = application.Config.ID;
            var keysToRemove = new List<string>();
            foreach (var (s, applications) in EventProcesses)
            {
                foreach (var app in applications)
                {
                    if (app.Config.ID == profileId)
                    {
                        keysToRemove.Add(s);
                    }
                }
            }

            foreach (var s in keysToRemove)
            {
                EventProcesses.Remove(s);
            }

            foreach (var exe in application.Config.ProcessNames)
            {
                if (exe.Equals(profileId)) continue;
                var processKey = exe.ToLower();
                AddEventProcess(processKey, application);
            }

            AddEventProcess(profileId, application);
        };
    }

    private void AddEventProcess(string processKey, Application application)
    {
        if (!EventProcesses.TryGetValue(processKey, out var applicationList))
        {
            applicationList = new SortedSet<Application>(new ApplicationPriorityComparer());
            EventProcesses[processKey] = applicationList;
        }

        applicationList.Add(application);
    }

    public void RemoveGenericProfile(string key)
    {
        if (!Events.TryGetValue(key, out var value)) return;
        if (value is not GenericApplication profile)
            return;
        Events.Remove(key);
        Global.Configuration.ProfileOrder.Remove(key);

        profile.Dispose();

        var path = profile.GetProfileFolderPath();
        if (Directory.Exists(path))
            Directory.Delete(path, true);
    }

    // Used to match a process's name and optional window title to a profile
    private Application? GetProfileFromProcessData(string processName, string processTitle)
    {
        var processNameProfile = GetProfileFromProcessName(processName);

        if (processNameProfile == null)
            return null;

        // Is title matching required?
        if (processNameProfile.Config.ProcessTitles != null)
        {
            var processTitleProfile = GetProfileFromProcessTitle(processTitle);

            if (processTitleProfile != null && processTitleProfile.Equals(processNameProfile))
            {
                return processTitleProfile;
            }
        }
        else
        {
            return processNameProfile;
        }

        return null;
    }

    public Application? GetProfileFromProcessName(string process)
    {
        return EventProcesses.GetValueOrDefault(process)?
            .FirstOrDefault(a => a.Settings?.IsEnabled ?? false);
    }


    private Application? GetProfileFromProcessTitle(string title)
    {
        return EventTitles.Where(entry => entry.Key.IsMatch(title))
            .Select(kv => kv.Value)
            .FirstOrDefault();
    }

    public Application? GetProfileFromAppId(string appid)
    {
        return !EventAppIDs.TryGetValue(appid, out var value) ? Events.GetValueOrDefault(appid) : value;
    }

    //TODO make this event based rather than polling
    /// <summary>Gets the current application.</summary>
    /// <param name="preview">Boolean indicating whether the application is selected because it is previewing (true)
    /// or because the process is open (false).</param>
    public Application GetCurrentProfile(out bool preview)
    {
        var processName = _activeProcessMonitor.Result.ProcessName.ToLower();
        var processTitle = _activeProcessMonitor.Result.ProcessTitle;
        Application? profile = null;
        Application? tempProfile;
        preview = false;

        //TODO: GetProfile that checks based on event type
        if ((tempProfile = GetProfileFromProcessData(processName, processTitle)) != null && tempProfile.IsEnabled)
            profile = tempProfile;
        //Don't check for it being Enabled as a preview should always end-up with the previewed profile regardless of it being disabled
        else if ((tempProfile = GetProfileFromProcessName(_previewModeProfileKey)) != null)
        {
            profile = tempProfile;
            preview = true;
        }
        else if (Global.Configuration.ExcludedPrograms.Contains(processName))
        {
            return DesktopProfile;
        }
        else if (Global.Configuration.AllowWrappersInBackground
                 && LfxState.IsWrapperConnected
                 && (tempProfile = GetProfileFromProcessName(LfxState.WrappedProcess)) != null 
                 && tempProfile.IsEnabled)
            profile = tempProfile;

        profile ??= DesktopProfile;

        return profile;
    }

    /// <summary>Gets the current application.</summary>
    public Application GetCurrentProfile() => GetCurrentProfile(out _);
    /// <summary>
    /// Returns a list of all profiles that should have their overlays active. This will include processes that running but not in the foreground.
    /// </summary>
    /// <returns></returns>
    public IEnumerable<Application> GetOverlayActiveProfiles()
    {
        // if previous overlay profiles are different from current, raise event
        var newOverlayProfiles = Events.Values
            .Where(_isOverlayActiveProfile)
            .Select(p => p.Config.ID);
        if (!OverlayActiveProfiles.SequenceEqual(newOverlayProfiles))
        {
            OverlayActiveProfiles = Events.Values
                .Where(_isOverlayActiveProfile)
                .Select(p => p.Config.ID)
                .ToImmutableHashSet();
            OverlayProfilesChanged?.Invoke(this, EventArgs.Empty);
        }
        
        return Events.Values.Where(_isOverlayActiveProfile);
    }

    /// <summary>KeyDown handler that checks the current application's profiles for keybinds.
    /// In the case of multiple profiles matching the keybind, it will pick the next one as specified in the Application.Profile order.</summary>
    private void CheckProfileKeybinds(object? sender, EventArgs e)
    {
        var profile = GetCurrentProfile();

        // Check profile is valid and do not switch profiles if the user is trying to enter a keybind
        if (Control_Keybind._ActiveKeybind != null) return;

        // Find all profiles that have their keybinds pressed
        var currentIndex = -1;
        var count = 0;
        foreach (var prof in profile.Profiles)
        {
            if (!prof.TriggerKeybind.IsPressed()) continue;
            if (currentIndex == -1 && prof == profile.Profile)
                currentIndex = count;
            count++;
        }

        // If at least one profile has it's key pressed\
        if (count == 0) return;
        // The target profile is the NEXT valid profile after the currently selected one
        // (or the first valid one if the currently selected one doesn't share this keybind)
        var nextIndex = (currentIndex + 1) % count;
        var idx = 0;
        foreach (var prof in profile.Profiles)
        {
            if (!prof.TriggerKeybind.IsPressed()) continue;
            if (idx == nextIndex)
            {
                profile.SwitchToProfile(prof);
                break;
            }
            idx++;
        }
    }

    private static void PutProfileTop(string profileId)
    {
        Global.Configuration.ProfileOrder.Remove(profileId);
        Global.Configuration.ProfileOrder.Insert(0, profileId);
    }

    private async Task ProfileInitAction()
    {
        if (_initTaskQueue.TryDequeue(out var action))
        {
            await action.Invoke();
            _profileInitThread.Trigger();
        }
    }

    private static void ProfileInitExceptionCallback(object? arg1, SingleThreadExceptionEventArgs arg2)
    {
        Global.logger.Fatal(arg2.Exception, "Profile load failed");
    }
    
    public void Dispose()
    {
        _initializeCancelSource.Cancel();
        _initializeCancelSource.Dispose();
        foreach (var (_, lightEvent) in Events)
            lightEvent.Dispose();
    }

    public async ValueTask DisposeAsync()
    {
        await _initializeCancelSource.CancelAsync();
        _initializeCancelSource.Dispose();
        foreach (var (_, lightEvent) in Events)
            lightEvent.Dispose();
    }
}