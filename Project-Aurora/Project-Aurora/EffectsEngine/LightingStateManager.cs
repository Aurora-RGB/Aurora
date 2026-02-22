using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using AuroraRgb.Devices;
using AuroraRgb.Modules.GameStateListen;
using AuroraRgb.Modules.ProcessMonitor;
using AuroraRgb.Profiles;
using AuroraRgb.Profiles.Desktop;
using AuroraRgb.Settings;
using AuroraRgb.Settings.Layers;
using AuroraRgb.Utils;
using Common.Utils;
using JetBrains.Annotations;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;
using Application = AuroraRgb.Profiles.Application;
using AssemblyExtensions = AuroraRgb.Utils.AssemblyExtensions;
using JsonSerializer = System.Text.Json.JsonSerializer;

namespace AuroraRgb.EffectsEngine;

public sealed class LightingStateManager : IDisposable
{
    private static readonly JsonSerializerOptions GameStateJsonSerializerOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
        TypeInfoResolverChain = { GameStateSourceGenerationContext.Default }
    };

    private Application? _currentEvent;
    public Application CurrentEvent => _currentEvent ?? ApplicationManager.DesktopProfile;

    private readonly HashSet<ILightEvent> _startedEvents = [];
    private readonly HashSet<ILightEvent> _updatedEvents = [];

    public Dictionary<Type, LayerHandlerMeta> LayerHandlers { get; } = new();

    public event EventHandler? PreUpdate;
    public event EventHandler? PostUpdate;

    public ApplicationManager ApplicationManager { get; }

    private readonly Task<PluginManager> _pluginManager;
    private readonly Task<DeviceManager> _deviceManager;
    private readonly Task<ActiveProcessMonitor> _activeProcessMonitor;

    private bool Initialized { get; set; }

    public LightingStateManager(Task<PluginManager> pluginManager,
        Task<DeviceManager> deviceManager, Task<ActiveProcessMonitor> activeProcessMonitor, Task<RunningProcessMonitor> runningProcessMonitor)
    {
        _pluginManager = pluginManager;
        _deviceManager = deviceManager;
        _activeProcessMonitor = activeProcessMonitor;
        
        ApplicationManager = new ApplicationManager(runningProcessMonitor, activeProcessMonitor);

        _updateTimer = new SingleConcurrentThread("LightingStateManager", TimerUpdate, ExceptionCallback);
    }

    private static void ExceptionCallback(object? sender, SingleThreadExceptionEventArgs eventArgs)
    {
        Global.logger.Error(eventArgs.Exception, "Unexpected error with LightingStateManager loop");
    }

    public async Task Initialize()
    {
        if (Initialized)
            return;

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
        
        await ApplicationManager.Initialize();

        await LoadPlugins();

        Initialized = true;
    }

    private async Task LoadPlugins()
    {
        (await _pluginManager).ProcessManager(this);
    }

    /// <summary>
    /// Manually registers a layer. Only needed externally.
    /// </summary>
    [PublicAPI]
    public bool RegisterLayer<T>() where T : ILayerHandler
    {
        var t = typeof(T);
        if (LayerHandlers.ContainsKey(t)) return false;
        var meta = t.GetCustomAttribute<LayerHandlerMetaAttribute>();
        LayerHandlers.Add(t, new LayerHandlerMeta(t, meta));
        return true;
    }

    private readonly SingleConcurrentThread _updateTimer;

    private long _nextProcessNameUpdate;
    private long _currentTick;

    private readonly EventIdle _idleE = new();

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

        var profile = ApplicationManager.GetCurrentProfile(out var preview);
        _currentEvent = profile;

        // If the current foreground process is excluded from Aurora, disable the lighting manager
        if (profile is Desktop && !profile.IsEnabled)
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
            if (ApplicationManager.DesktopProfile.IsOverlayEnabled)
            {
                ApplicationManager.DesktopProfile.UpdateOverlayLights(_drawnFrame);
            }
            
            foreach (var @event in ApplicationManager.GetOverlayActiveProfiles())
            {
                StartEvent(@event);
                @event.UpdateOverlayLights(_drawnFrame);
            }

            //Add the Light event that we're previewing to be rendered as an overlay (assuming it's not already active)
            if (preview && Global.Configuration.OverlaysInPreview && !ApplicationManager.GetOverlayActiveProfiles().Contains(profile))
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

    public void JsonGameStateUpdate(object? sender, JsonGameStateEventArgs eventArgs)
    {
        var gameId = eventArgs.GameId;
        var profile = ApplicationManager.GetProfileFromAppId(gameId);
        if (profile == null)
        {
            return;
        }

        var gameStateType = profile.Config.GameStateType;
        var jsonStream = eventArgs.Json;
        if (JsonSerializer.Deserialize(jsonStream, gameStateType, GameStateJsonSerializerOptions) is not IGameState gameState)
        {
            return;
        }

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
            if ((profile = ApplicationManager.GetProfileFromAppId(appid)) == null && (profile = ApplicationManager.GetProfileFromProcessName(name)) == null)
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
        var profile = ApplicationManager.GetProfileFromProcessName(process);
        profile?.ResetGameState();
    }

    public void Dispose()
    {
        ApplicationManager.Dispose();
        _updateTimer.Dispose(200);
    }

    public async Task DisposeAsync()
    {
        await ApplicationManager.DisposeAsync();
        _updateTimer.Dispose(200);
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