using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using AuroraRgb.Controls;
using AuroraRgb.Devices;
using AuroraRgb.EffectsEngine;
using AuroraRgb.Modules;
using AuroraRgb.Modules.GameStateListen;
using AuroraRgb.Modules.ProcessMonitor;
using AuroraRgb.Profiles.Desktop;
using AuroraRgb.Profiles.Generic_Application;
using AuroraRgb.Settings;
using AuroraRgb.Settings.Layers;
using AuroraRgb.Utils;
using Common.Utils;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;
using AssemblyExtensions = AuroraRgb.Utils.AssemblyExtensions;
using JsonSerializer = System.Text.Json.JsonSerializer;

namespace AuroraRgb.Profiles;

public sealed class ApplicationInitializedEventArgs(Application application) : EventArgs
{
    public Application Application { get; } = application;
}

public sealed class LightingStateManager : IDisposable
{
    private static readonly JsonSerializerOptions GameStateJsonSerializerOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
        TypeInfoResolverChain = { GameStateSourceGenerationContext.Default }
    };

    public event EventHandler<ApplicationInitializedEventArgs>? EventAdded;
    public Dictionary<string, Application> Events { get; } = new() { { "desktop", new Desktop.Desktop() } };

    private Desktop.Desktop DesktopProfile => (Desktop.Desktop)Events["desktop"];
    private Application? _currentEvent;
    public Application CurrentEvent => _currentEvent ?? DesktopProfile;

    private readonly HashSet<ILightEvent> _startedEvents = [];
    private readonly HashSet<ILightEvent> _updatedEvents = [];

    private Dictionary<string, SortedSet<Application>> EventProcesses { get; } = new();
    private Dictionary<Regex, Application> EventTitles { get; } = new();
    private Dictionary<string, Application> EventAppIDs { get; } = new();
    public Dictionary<Type, LayerHandlerMeta> LayerHandlers { get; } = new();

    public event EventHandler? PreUpdate;
    public event EventHandler? PostUpdate;

    private readonly Func<Application, bool> _isOverlayActiveProfile;

    private readonly Task<PluginManager> _pluginManager;
    private readonly Task<IpcListener?> _ipcListener;
    private readonly Task<DeviceManager> _deviceManager;
    private readonly Task<ActiveProcessMonitor> _activeProcessMonitor;
    private readonly Task<RunningProcessMonitor> _runningProcessMonitor;

    private bool Initialized { get; set; }
    private readonly CancellationTokenSource _initializeCancelSource = new();
    
    private readonly ConcurrentQueue<Func<Task>> _initTaskQueue = new();
    private readonly SingleConcurrentThread _profileInitThread;

    public LightingStateManager(Task<PluginManager> pluginManager, Task<IpcListener?> ipcListener,
        Task<DeviceManager> deviceManager, Task<ActiveProcessMonitor> activeProcessMonitor, Task<RunningProcessMonitor> runningProcessMonitor)
    {
        _pluginManager = pluginManager;
        _ipcListener = ipcListener;
        _deviceManager = deviceManager;

        _activeProcessMonitor = activeProcessMonitor;
        _runningProcessMonitor = runningProcessMonitor;
        Predicate<string> processRunning = ProcessRunning;
        _isOverlayActiveProfile = evt => evt.IsOverlayEnabled &&
                                         Array.Exists(evt.Config.ProcessNames, processRunning);
        
        _profileInitThread = new SingleConcurrentThread("ProfileInit", ProfileInitAction, ProfileInitExceptionCallback);

        _updateTimer = new SingleConcurrentThread("LightingStateManager", TimerUpdate, ExceptionCallback);

        bool ProcessRunning(string name) => _runningProcessMonitor.Result.IsProcessRunning(name);
    }

    private static void ProfileInitExceptionCallback(object? arg1, SingleThreadExceptionEventArgs arg2)
    {
        Global.logger.Fatal(arg2.Exception, "Profile load failed");
    }

    private async Task ProfileInitAction()
    {
        if (_initTaskQueue.TryDequeue(out var action))
        {
            await action.Invoke();
            _profileInitThread.Trigger();
        }
    }

    private static void ExceptionCallback(object? sender, SingleThreadExceptionEventArgs eventArgs)
    {
        Global.logger.Error(eventArgs.Exception, "Unexpected error with LightingStateManager loop");
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

        // Register all layer types that are in the Aurora.Settings.Layers namespace.
        // Do not register all that are inside the assembly since some are application-specific (e.g. minecraft health layer)
        var layerTypes = from type in AssemblyExtensions.GetLoadableTypes(Assembly.GetExecutingAssembly())
            where type.GetInterfaces().Contains(typeof(ILayerHandler))
            let name = type.Name.CamelCaseToSpaceCase()
            let meta = type.GetCustomAttribute<LayerHandlerMetaAttribute>()
            where !type.IsGenericType
            where meta is not { Exclude: true }
            select (type, meta);
        foreach (var (type, meta) in layerTypes)
            LayerHandlers.Add(type, new LayerHandlerMeta(type, meta));

        await DesktopProfile.Initialize(cancellationToken);
        LoadSettings();
        await LoadPlugins();

        // Listen for profile keybind triggers
        (await InputsModule.InputEvents).KeyDown += CheckProfileKeybinds;

        Initialized = true;
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
            return Array.Empty<Application>();
        }

        var userApps = from dir in Directory.EnumerateDirectories(additionalProfilesPath)
            where File.Exists(Path.Combine(dir, "settings.json"))
            select Path.GetFileName(dir);

        return userApps.Select(processName => new GenericApplication(processName));
    }

    private async Task LoadPlugins()
    {
        (await _pluginManager).ProcessManager(this);
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
        PutProfileTop("chroma");
        PutProfileTop("desktop");
    }

    private static void PutProfileTop(string profileId)
    {
        Global.Configuration.ProfileOrder.Remove(profileId);
        Global.Configuration.ProfileOrder.Insert(0, profileId);
    }

    public async Task RegisterEvent(Application application)
    {
        var profileId = application.Config.ID;
        if (string.IsNullOrWhiteSpace(profileId) || !Events.TryAdd(profileId, application)) return;

        foreach (var exe in application.Config.ProcessNames)
        {
            if (!EventProcesses.TryGetValue(exe, out var applicationList))
            {
                applicationList = new SortedSet<Application>(new ApplicationPriorityComparer());
                EventProcesses[exe] = applicationList;
            }
            applicationList.Add(application);
        }
        EventProcesses[application.Config.ID] = [application];

        application.Config.ProcessNamesChanged += (_, _) =>
        {
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
                if (!EventProcesses.TryGetValue(processKey, out var applicationList))
                {
                    applicationList = new SortedSet<Application>(new ApplicationPriorityComparer());
                    EventProcesses[processKey] = applicationList;
                }
                applicationList.Add(application);
            }
        };

        if (application.Config.ProcessTitles != null)
            foreach (var titleRx in application.Config.ProcessTitles)
                EventTitles.Add(titleRx, application);

        if (!string.IsNullOrWhiteSpace(application.Config.AppID))
            EventAppIDs.Add(application.Config.AppID, application);

        if (!Global.Configuration.ProfileOrder.Contains(profileId))
        {
            Global.Configuration.ProfileOrder.Add(profileId);
        }

        if (Initialized)
            await application.Initialize(_initializeCancelSource.Token);
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

    private Application? GetProfileFromProcessName(string process)
    {
        return EventProcesses.GetValueOrDefault(process)?.First();
    }

    /// <summary>
    /// Manually registers a layer. Only needed externally.
    /// </summary>
    public bool RegisterLayer<T>() where T : ILayerHandler
    {
        var t = typeof(T);
        if (LayerHandlers.ContainsKey(t)) return false;
        var meta = t.GetCustomAttribute<LayerHandlerMetaAttribute>();
        LayerHandlers.Add(t, new LayerHandlerMeta(t, meta));
        return true;
    }

    private Application? GetProfileFromProcessTitle(string title)
    {
        return EventTitles.Where(entry => entry.Key.IsMatch(title))
            .Select(kv => kv.Value)
            .FirstOrDefault();
    }

    private Application? GetProfileFromAppId(string appid)
    {
        return !EventAppIDs.TryGetValue(appid, out var value) ? Events.GetValueOrDefault(appid) : value;
    }

    private readonly SingleConcurrentThread _updateTimer;

    private long _nextProcessNameUpdate;
    private long _currentTick;
    private string _previewModeProfileKey = "";

    private readonly EventIdle _idleE = new();

    public string? PreviewProfileKey {
        get => _previewModeProfileKey;
        set => _previewModeProfileKey = value ?? string.Empty;
    }

    private readonly Stopwatch _watch = new();

    public Task InitUpdate()
    {
        _watch.Start();
        _updateTimer?.Trigger();
        return Task.CompletedTask;
    }

    private void TimerUpdate()
    {
        GC.WaitForPendingFinalizers();
        if (Debugger.IsAttached)
        {
            Thread.Sleep(40);
        }

        if (Global.isDebug)
            Update();
        else
        {
            try
            {
                Update();
            }
            catch (Exception exc)
            {
                Global.logger.Error(exc, "ProfilesManager.Update() Exception:");
                //TODO make below non-blocking
                MessageBox.Show("Error while updating light effects: " + exc.Message);
            }
        }
        _currentTick += _watch.ElapsedMilliseconds;
        var millisecondsTimeout = Math.Max(Global.Configuration.UpdateDelay - _watch.ElapsedMilliseconds, 1);
        Thread.Sleep(TimeSpan.FromMilliseconds(millisecondsTimeout));
        _updateTimer.Trigger();
        _watch.Restart();
    }

    private void UpdateProcess()
    {
        var pollingEnabled = Global.Configuration.DetectionMode is ApplicationDetectionMode.ForegroundApp or ApplicationDetectionMode.EventsAndForeground;
        if (!pollingEnabled || _currentTick < _nextProcessNameUpdate) return;
        _activeProcessMonitor.Result.UpdateActiveProcessPolling();
        _nextProcessNameUpdate = _currentTick + 2000L;
    }

    private void UpdateIdleEffects(EffectFrame newFrame)
    {
        if (!User32.GetLastInputInfoOut(out var lastInput)) return;
        var idleTime = Environment.TickCount - lastInput.dwTime;

        if (idleTime < Global.Configuration.IdleDelay * 60 * 1000) return;
        if (Global.Configuration.TimeBasedDimmingEnabled &&
            Time.IsCurrentTimeBetween(Global.Configuration.TimeBasedDimmingStartHour,
                Global.Configuration.TimeBasedDimmingStartMinute,
                Global.Configuration.TimeBasedDimmingEndHour,
                Global.Configuration.TimeBasedDimmingEndMinute)) return;
        UpdateEvent(_idleE, newFrame);
    }

    private void UpdateEvent(ILightEvent @event, EffectFrame frame)
    {
        StartEvent(@event);
        @event.UpdateLights(frame);
    }

    private void StartEvent(ILightEvent @event)
    {
        _updatedEvents.Add(@event);

        // Skip if event was already started
        if (!_startedEvents.Add(@event)) return;

        @event.OnStart();
    }

    private void StopUnUpdatedEvents()
    {
        // Skip if there are no started events or started events are the same since last update
        if (_startedEvents.Count == 0 || _startedEvents.SequenceEqual(_updatedEvents)) return;

        var eventsToStop = _startedEvents.Except(_updatedEvents).ToList();
        foreach (var eventToStop in eventsToStop)
            eventToStop.OnStop();

        _startedEvents.Clear();
        foreach (var updatedEvent in _updatedEvents)
        {
            _startedEvents.Add(updatedEvent);
        }
    }

    private bool _profilesDisabled;
    private readonly EffectFrame _drawnFrame = new();

    private static readonly DefaultContractResolver ContractResolver = new()
    {
        NamingStrategy = new SnakeCaseNamingStrategy()
        {
            OverrideSpecifiedNames = false
        }
    };

    private void Update()
    {
        PreUpdate?.Invoke(this, EventArgs.Empty);
        _updatedEvents.Clear();

        //Blackout. TODO: Cleanup this a bit. Maybe push blank effect frame to keyboard incase it has existing stuff displayed
        var dimmingStartTime = new TimeSpan(Global.Configuration.TimeBasedDimmingStartHour,
            Global.Configuration.TimeBasedDimmingStartMinute, 0);
        var dimmingEndTime = new TimeSpan(Global.Configuration.TimeBasedDimmingEndHour, 
            Global.Configuration.TimeBasedDimmingEndMinute, 0);
        if (Global.Configuration.TimeBasedDimmingEnabled &&
            Time.IsCurrentTimeBetween(dimmingStartTime, dimmingEndTime))
        {
            var blackFrame = new EffectFrame();
            Global.effengine.PushFrame(blackFrame);
            StopUnUpdatedEvents();
            return;
        }

        UpdateProcess();

        var profile = GetCurrentProfile(out var preview);
        _currentEvent = profile;

        // If the current foreground process is excluded from Aurora, disable the lighting manager
        if (profile is Desktop.Desktop && !profile.IsEnabled)
        {
            if (!_profilesDisabled)
            {
                StopUnUpdatedEvents();
                Global.effengine.PushFrame(_drawnFrame);
                _deviceManager.Result.ShutdownDevices();
            }

            _profilesDisabled = true;
            return;
        }

        if (_profilesDisabled)
        {
            _deviceManager.Result.InitializeDevices();
            _profilesDisabled = false;
        }

        //Need to do another check in case Desktop is disabled or the selected preview is disabled
        if (profile.IsEnabled)
            UpdateEvent(profile, _drawnFrame);

        // Overlay layers
        if (!preview || Global.Configuration.OverlaysInPreview)
        {
            if (DesktopProfile.IsOverlayEnabled)
            {
                DesktopProfile.UpdateOverlayLights(_drawnFrame);
            }
            
            foreach (var @event in GetOverlayActiveProfiles())
            {
                StartEvent(@event);
                @event.UpdateOverlayLights(_drawnFrame);
            }

            //Add the Light event that we're previewing to be rendered as an overlay (assuming it's not already active)
            if (preview && Global.Configuration.OverlaysInPreview && !GetOverlayActiveProfiles().Contains(profile))
            {
                StartEvent(profile);
                profile.UpdateOverlayLights(_drawnFrame);
            }

            if (Global.Configuration.IdleType != IdleEffects.None)
            {
                UpdateIdleEffects(_drawnFrame);
            }
        }

        Global.effengine.PushFrame(_drawnFrame);

        StopUnUpdatedEvents();
        PostUpdate?.Invoke(this, EventArgs.Empty);
    }

    /// <summary>Gets the current application.</summary>
    /// <param name="preview">Boolean indicating whether the application is selected because it is previewing (true)
    /// or because the process is open (false).</param>
    private Application GetCurrentProfile(out bool preview)
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
        return Events.Values.Where(_isOverlayActiveProfile);
    }

    /// <summary>KeyDown handler that checks the current application's profiles for keybinds.
    /// In the case of multiple profiles matching the keybind, it will pick the next one as specified in the Application.Profile order.</summary>
    private void CheckProfileKeybinds(object? sender, EventArgs e)
    {
        var profile = GetCurrentProfile();

        // Check profile is valid and do not switch profiles if the user is trying to enter a keybind
        if (profile is not Application application || Control_Keybind._ActiveKeybind != null) return;
        // Find all profiles that have their keybinds pressed
        var possibleProfiles = application.Profiles
            .Where(prof => prof.TriggerKeybind.IsPressed())
            .ToList();

        // If atleast one profile has it's key pressed
        if (possibleProfiles.Count <= 0) return;
        // The target profile is the NEXT valid profile after the currently selected one
        // (or the first valid one if the currently selected one doesn't share this keybind)
        var trg = (possibleProfiles.IndexOf(application.Profile) + 1) % possibleProfiles.Count;
        application.SwitchToProfile(possibleProfiles[trg]);
    }

    public void JsonGameStateUpdate(object? sender, JsonGameStateEventArgs eventArgs)
    {
        var gameId = eventArgs.GameId;
        var profile = GetProfileFromAppId(gameId);
        if (profile == null)
        {
            return;
        }

        var gameStateType = profile.Config.GameStateType;
        var json = eventArgs.Json;

        var gameState = JsonSerializer.Deserialize(json, gameStateType, GameStateJsonSerializerOptions) as IGameState;
        
        profile.SetGameState(gameState);
    }

    public void GameStateUpdate(object? sender, IGameState gs)
    {
        try
        {
            if (gs is not NewtonsoftGameState newtonsoftGameState)
            {
                return;
            }
            
            if (!newtonsoftGameState.Announce)
            {
                 return;   
            }
            
            var provider = JObject.Parse(newtonsoftGameState.GetNode("provider"));
            var appid = provider.GetValue("appid").ToString();
            var name = provider.GetValue("name").ToString().ToLowerInvariant();

            Application? profile;
            if ((profile = GetProfileFromAppId(appid)) == null && (profile = GetProfileFromProcessName(name)) == null)
            {
                return;
            }

            // profile supports System.Text.Json but we received data on old endpoint
            if (!profile.Config.GameStateType.IsAssignableTo(typeof(NewtonsoftGameState)))
            {
                var njGameState = JsonConvert.DeserializeObject(newtonsoftGameState.Json, profile.Config.GameStateType, new JsonSerializerSettings()
                {
                    ContractResolver = ContractResolver,
                }) as IGameState;
                profile.SetGameState(njGameState);
                return;
            }

            var gameState = (NewtonsoftGameState)Activator.CreateInstance(profile.Config.GameStateType, newtonsoftGameState.Json);
            profile.SetGameState(gameState);
        }
        catch (Exception e)
        {
            Global.logger.Warning(e, "Exception during GameStateUpdate(), error: ");
            if (Debugger.IsAttached)
            {
                throw;
            }
        }
    }

    public void ResetGameState(object? sender, string process)
    {
        var profile = GetProfileFromProcessName(process);
        profile?.ResetGameState();
    }

    public void Dispose()
    {
        _initializeCancelSource.Cancel();
        _initializeCancelSource.Dispose();
        _updateTimer.Dispose(200);
        foreach (var (_, lightEvent) in Events)
            lightEvent.Dispose();
    }

    public async Task DisposeAsync()
    {
        await _initializeCancelSource.CancelAsync();
        _initializeCancelSource.Dispose();
        _updateTimer.Dispose(200);
        foreach (var (_, lightEvent) in Events)
            lightEvent.Dispose();
    }
}

/// <summary>
/// POCO that stores data about a type of layer.
/// </summary>
public class LayerHandlerMeta
{

    /// <summary>Creates a new LayerHandlerMeta object from the given meta attribute and type.</summary>
    public LayerHandlerMeta(Type type, LayerHandlerMetaAttribute? attribute)
    {
        Name = attribute?.Name ?? type.Name.CamelCaseToSpaceCase().TrimEndStr(" Layer Handler");
        Type = type;
        // if the layer is in the Aurora.Settings.Layers namespace, make the IsDefault true unless otherwise specified.
        // If it is in another namespace, it's probably a custom application layer and so make IsDefault false unless otherwise specified
        IsDefault = attribute?.IsDefault ?? type.Namespace == "AuroraRgb.Settings.Layers";
        Order = attribute?.Order ?? 0;
    }

    public string Name { get; }
    public Type Type { get; }
    public bool IsDefault { get; }
    public int Order { get; }
}


/// <summary>
/// Attribute to provide additional meta data about layers for them to be registered.
/// </summary>
[AttributeUsage(AttributeTargets.Class, Inherited = false)]
public class LayerHandlerMetaAttribute : Attribute
{
    /// <summary>A different name for the layer. If not specified, will automatically take it from the layer's class name.</summary>
    public string Name { get; set; }

    /// <summary>If true, this layer will be excluded from automatic registration. Default false.</summary>
    public bool Exclude { get; set; }

    /// <summary>If true, this layer will be registered as a 'default' layer for all applications. Default true.</summary>
    public bool IsDefault { get; set; }

    /// <summary>A number used when ordering the layer entry in the list.
    /// Only to be used for layers that need to appear at the top/bottom of the list.</summary>
    public int Order { get; set; }
}