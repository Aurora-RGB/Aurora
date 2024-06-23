using System.Threading.Tasks;
using AuroraRgb.Devices;
using AuroraRgb.Modules.GameStateListen;
using AuroraRgb.Modules.ProcessMonitor;
using AuroraRgb.Profiles;
using AuroraRgb.Settings;

namespace AuroraRgb.Modules;

public sealed class LightingStateManagerModule(
    Task<PluginManager> pluginManager,
    Task<IpcListener?> listener,
    Task<AuroraHttpListener?> httpListener,
    Task<DeviceManager> deviceManager,
    Task<ActiveProcessMonitor> activeProcessMonitor,
    Task<RunningProcessMonitor> runningProcessMonitor
) : AuroraModule
{
    private static readonly TaskCompletionSource<LightingStateManager> TaskSource = new(TaskCreationOptions.RunContinuationsAsynchronously);

    private LightingStateManager? _manager;

    public static Task<LightingStateManager> LightningStateManager => TaskSource.Task;

    protected override async Task Initialize()
    {
        Global.logger.Information("Loading Applications");
        var lightingStateManager = new LightingStateManager(pluginManager, listener, deviceManager, activeProcessMonitor, runningProcessMonitor);
        _manager = lightingStateManager;
        Global.LightingStateManager = lightingStateManager;
        await lightingStateManager.Initialize();

        TaskSource.SetResult(lightingStateManager);

        var ipcListener = await listener;
        if (ipcListener != null)
        {
            ipcListener.NewGameState += lightingStateManager.GameStateUpdate;
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
    }
    
    public override async ValueTask DisposeAsync()
    {
        if (_manager == null)
            return;
        await _manager.DisposeAsync();

        var ipcListener = await listener;
        if (ipcListener != null)
        {
            ipcListener.NewGameState -= _manager.GameStateUpdate;
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