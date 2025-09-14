using System;
using System.ComponentModel;
using System.Threading.Tasks;
using AuroraRgb.Modules.ProcessMonitor;
using iCUE_ReverseEngineer.Icue;

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

    private IcueServerStatus _serverStatus = IcueServerStatus.Disabled;

    public IcueServerStatus ServerStatus
    {
        get => _serverStatus;
        private set
        {
            if (_serverStatus == value) return;
            _serverStatus = value;
            StatusChanged?.Invoke(this, EventArgs.Empty);
        }
    }

    private IcueServer? _icueServer;
    private GameHandler? _gameHandler;

    public void RunServer(RunningProcessMonitor processMonitor)
    {
        Sdk.RunningProcessMonitor = processMonitor;
        var runApproved = IcueInstallationUtils.IsIcueInstalled() && IcueInstallationUtils.IsIcueAutorunEnabled();
        if (runApproved || processMonitor.IsProcessRunning(IcueInstallationUtils.IcueExe))
        {
            ServerStatus = IcueServerStatus.Conflicted;
            return;
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
        _gameHandler = gameHandler;
        Gsi.SetGsiHandler(gameHandler.GsiHandler);
        Sdk.SetSdkHandler(gameHandler.SdkHandler, gameHandler.GamePid);

        gameHandler.GameDisconnected += OnGameDisconnected;
    }

    private void OnGameDisconnected(object? sender, EventArgs e)
    {
        _gameHandler?.Dispose();
        _gameHandler = null;
        Gsi.ClearGsiHandler();
        Sdk.ClearSdkHandler();
    }

    public void Dispose()
    {
        ServerStatus = IcueServerStatus.Disabled;
        if (_icueServer == null) return;
        _icueServer.GameConnected -= OnGameConnected;
        _icueServer.Dispose();
    }

    public async ValueTask DisposeAsync()
    {
        ServerStatus = IcueServerStatus.Disabled;
        if (_icueServer == null)
        {
            return;
        }

        _icueServer.GameConnected -= OnGameConnected;
        await _icueServer.DisposeAsync();
    }
}