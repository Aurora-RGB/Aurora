using System.Threading.Tasks;
using AuroraRgb.Devices;
using AuroraRgb.EffectsEngine;
using AuroraRgb.Modules.GameStateListen;
using AuroraRgb.Modules.ProcessMonitor;
using AuroraRgb.Profiles;
using AuroraRgb.Settings;
using AuroraRgb.Settings.Overrides.Logic;

namespace AuroraRgb.Modules;

public sealed class LightingStateManagerModule(
    Task<PluginManager> pluginManager,
    Task<IpcListener?> listener,
    Task<AuroraHttpListener?> httpListener,
    Task<DeviceManager> deviceManager,
    Task<ActiveProcessMonitor> activeProcessMonitor,
    Task<RunningProcessMonitor> runningProcessMonitor,
    DevicesModule devicesModule) : AuroraModule
{
    private static readonly TaskCompletionSource<LightingStateManager> TaskSource = new(TaskCreationOptions.RunContinuationsAsynchronously);

    private LightingStateManager? _manager;

    public static Task<LightingStateManager> LightningStateManager => TaskSource.Task;

    protected override async Task Initialize()
    {
        Global.logger.Information("Loading Applications");
        var lightingStateManager = new LightingStateManager(pluginManager, deviceManager, activeProcessMonitor, runningProcessMonitor);
        _manager = lightingStateManager;

        Global.LightingStateManager = lightingStateManager;
        Global.effengine = new Effects(devicesModule.DeviceManager);

        await lightingStateManager.Initialize();

        TaskSource.SetResult(lightingStateManager);

        var ipcListener = await listener;
        if (ipcListener != null)
        {
            ipcListener.WrapperConnectionClosed += lightingStateManager.ResetGameState;
        }

        var httpListener1 = await httpListener;
        if (httpListener1 != null)
        {
            httpListener1.NewGameState += lightingStateManager.GameStateUpdate;
            httpListener1.NewJsonGameState += lightingStateManager.JsonGameStateUpdate;
        }
        Global.logger.Information("Loaded Applications");
        await lightingStateManager.InitUpdate();

        // warmup static value
        EvaluatableRegistry.Get();
    }
    
    public override async ValueTask DisposeAsync()
    {
        if (_manager == null)
            return;
        await _manager.DisposeAsync();

        var ipcListener = await listener;
        if (ipcListener != null)
        {
            ipcListener.WrapperConnectionClosed -= _manager.ResetGameState;
        }

        var httpListener1 = await httpListener;
        if (httpListener1 != null)
        {
            httpListener1.NewGameState -= _manager.GameStateUpdate;
            httpListener1.NewJsonGameState -= _manager.JsonGameStateUpdate;
        }
        
        Global.LightingStateManager = null;
        _manager = null;
    }
}