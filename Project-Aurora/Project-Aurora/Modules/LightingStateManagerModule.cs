﻿using System.Threading.Tasks;
using Aurora.Devices;
using Aurora.Modules.GameStateListen;
using Aurora.Profiles;
using Aurora.Settings;
using Lombok.NET;

namespace Aurora.Modules;

public sealed partial class LightingStateManagerModule : AuroraModule
{
    private readonly Task<PluginManager> _pluginManager;
    private readonly Task<IpcListener?> _ipcListener;
    private readonly Task<AuroraHttpListener?> _httpListener;
    private readonly Task<DeviceManager> _deviceManager;

    private static readonly TaskCompletionSource<LightingStateManager> TaskSource = new(TaskCreationOptions.RunContinuationsAsynchronously);

    private LightingStateManager? _manager;

    public static Task<LightingStateManager> LightningStateManager => TaskSource.Task;

    public LightingStateManagerModule(Task<PluginManager> pluginManager, Task<IpcListener?> ipcListener,
        Task<AuroraHttpListener?> httpListener, Task<DeviceManager> deviceManager)
    {
        _pluginManager = pluginManager;
        _ipcListener = ipcListener;
        _httpListener = httpListener;
        _deviceManager = deviceManager;
    }

    protected override async Task Initialize()
    {
        Global.logger.Information("Loading Applications");
        var lightingStateManager = new LightingStateManager(_pluginManager, _ipcListener, _deviceManager);
        _manager = lightingStateManager;
        Global.LightingStateManager = lightingStateManager;
        await lightingStateManager.Initialize();

        TaskSource.SetResult(lightingStateManager);

        var ipcListener = await _ipcListener;
        if (ipcListener != null)
        {
            ipcListener.NewGameState += lightingStateManager.GameStateUpdate;
            ipcListener.WrapperConnectionClosed += lightingStateManager.ResetGameState;
        }

        var httpListener = await _httpListener;
        if (httpListener != null)
        {
            httpListener.NewGameState += lightingStateManager.GameStateUpdate;
        }
        await lightingStateManager.InitUpdate();
    }

    [Async]
    public override void Dispose()
    {
        _manager?.Dispose();
        Global.LightingStateManager = null;
        _manager = null;
    }
}