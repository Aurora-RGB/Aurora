using System;
using System.ComponentModel;
using System.Threading.Tasks;
using AuroraRgb.Modules.ProcessMonitor;
using iCUE_ReverseEngineer.Icue;
using iCUE_ReverseEngineer.Icue.Gsi;

namespace AuroraRgb.Modules.Icue;

public enum IcueServerStatus
{
    [Description("Disabled")]
    Disabled,
    [Description("Waiting for Games")]
    Waiting,
    [Description("Pipes in Use")]
    PipesInUse,
    Conflicted,
}

public sealed class AuroraIcueServer : IDisposable, IAsyncDisposable
{
    public IcueGsi Gsi { get; } = new();
    public IcueSdk Sdk { get; } = new();

    public event EventHandler? StatusChanged;

    public IcueServerStatus ServerStatus
    {
        get;
        private set
        {
            if (field == value) return;
            field = value;
            StatusChanged?.Invoke(this, EventArgs.Empty);
        }
    } = IcueServerStatus.Disabled;

    private IcueServer? _icueServer;
    private GameHandler? _gameHandler;
    private RunningProcessMonitor? _processMonitor;

    public void RunServer(RunningProcessMonitor processMonitor)
    {
        _processMonitor = processMonitor;
        
        processMonitor.ProcessStarted += ProcessMonitorOnProcessStarted;
        processMonitor.ProcessStopped += ProcessMonitorOnProcessStopped;
        
        var runApproved = IcueInstallationUtils.IsIcueInstalled() && IcueInstallationUtils.IsIcueAutorunEnabled();
        if (runApproved || processMonitor.IsProcessRunning(IcueInstallationUtils.IcueExe))
        {
            ServerStatus = IcueServerStatus.Conflicted;
            return;
        }
        
        TryStartServer();
    }

    private void ProcessMonitorOnProcessStarted(object? sender, ProcessStarted e)
    {
        if (e.ProcessName != IcueInstallationUtils.IcueExe)
        {
            return;
        }

        switch (ServerStatus)
        {
            case IcueServerStatus.Conflicted:
            case IcueServerStatus.Disabled:
            case IcueServerStatus.PipesInUse:
                return;
            case IcueServerStatus.Waiting:
                // proceed to close the server
                break;
        }
        ServerStatus = IcueServerStatus.Conflicted;
            
        if (_icueServer == null) return;
        _icueServer.GameConnected -= OnGameConnected;
        _icueServer.Dispose();
        _icueServer = null;
    }

    private void ProcessMonitorOnProcessStopped(object? sender, ProcessStopped e)
    {
        if (e.ProcessName != IcueInstallationUtils.IcueExe)
        {
            return;
        }
        
        switch (ServerStatus)
        {
            case IcueServerStatus.Waiting:
            case IcueServerStatus.Disabled:
                return;
            case IcueServerStatus.Conflicted:
            case IcueServerStatus.PipesInUse:
                // Aurora's turn to use the pipes
                break;
        }

        var runApproved = IcueInstallationUtils.IsIcueInstalled() && IcueInstallationUtils.IsIcueAutorunEnabled();
        if (runApproved)
        {
            ServerStatus = IcueServerStatus.Conflicted;
        }

        TryStartServer();
    }

    private void TryStartServer()
    {
        try
        {
            // Dispose previous just in case
            if (_icueServer != null)
            {
                _icueServer.Dispose();
                _icueServer.GameConnected -= OnGameConnected;
                _icueServer = null;
            }

            _icueServer = new IcueServer();
            _icueServer.GameConnected += OnGameConnected;
            _icueServer.Run();
            ServerStatus = IcueServerStatus.Waiting;
        }
        catch (Exception)
        {
            // Most likely pipes are in use by iCUE at this moment
            _icueServer = null;
            ServerStatus = IcueServerStatus.PipesInUse;
        }
    }

    private void OnGameConnected(object? sender, GameHandler gameHandler)
    {
        Global.logger.Information("[iCUE] Game connected: PID {Pid}", gameHandler.GamePid);
        _gameHandler = gameHandler;
        gameHandler.SdkHandler.GameConnected += SdkHandlerOnGameConnected;
        gameHandler.GsiHandler.GameConnected += GsiHandlerOnGameConnected;

        gameHandler.GameDisconnected += OnGameDisconnected;
    }

    private void SdkHandlerOnGameConnected(object? sender, EventArgs e)
    {
        Global.logger.Information("[iCUE] SDK connected: PID {Pid}", _gameHandler!.GamePid);
        Sdk.SetSdkHandler(_gameHandler!.SdkHandler, _gameHandler.GamePid);
    }

    private void GsiHandlerOnGameConnected(object? sender, IcueGsiConnectionEventArgs e)
    {
        Global.logger.Information("[iCUE] GSI connected: PID {Pid}, Game {GameName}", _gameHandler!.GamePid, e.GameName);
        Gsi.SetGsiHandler(_gameHandler!.GsiHandler, e);
    }

    private void OnGameDisconnected(object? sender, EventArgs e)
    {
        if (_gameHandler != null)
        {
            _gameHandler.SdkHandler.GameConnected -= SdkHandlerOnGameConnected;
            _gameHandler.GsiHandler.GameConnected -= GsiHandlerOnGameConnected;

            _gameHandler.GameDisconnected -= OnGameDisconnected;
            
            _gameHandler.Dispose();
        }
        
        _gameHandler = null;
        Gsi.ClearGsiHandler();
        Sdk.ClearSdkHandler();
    }

    public void Stop()
    {
        if (_processMonitor != null)
        {
            _processMonitor.ProcessStarted -= ProcessMonitorOnProcessStarted;
            _processMonitor.ProcessStopped -= ProcessMonitorOnProcessStopped;
        }
 
        ServerStatus = IcueServerStatus.Disabled;
        if (_icueServer == null) return;
        _icueServer.GameConnected -= OnGameConnected;
        _icueServer.Dispose();
        _icueServer = null;
    }

    public async Task StopAsync()
    {
        if (_processMonitor != null)
        {
            _processMonitor.ProcessStarted -= ProcessMonitorOnProcessStarted;
            _processMonitor.ProcessStopped -= ProcessMonitorOnProcessStopped;
        }

        ServerStatus = IcueServerStatus.Disabled;
        if (_icueServer == null) return;
        _icueServer.GameConnected -= OnGameConnected;
        await _icueServer.DisposeAsync();
        _icueServer = null;
    }

    public void Dispose()
    {
        Stop();
    }

    public async ValueTask DisposeAsync()
    {
        await StopAsync();
    }
}