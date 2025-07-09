using System;
using System.IO;
using System.IO.Pipes;
using System.Security.AccessControl;
using System.Security.Principal;
using System.Text.Json;
using System.Threading.Tasks;
using Common;

namespace AuroraRgb.Modules.GameStateListen;

public class IpcListener
{
    private bool _isRunning;

    public event EventHandler<string>? WrapperConnectionClosed;

    public event EventHandler<string>? AuroraCommandReceived; 

    /// <summary>
    /// Returns whether or not the wrapper is connected through IPC
    /// </summary>
    public bool IsWrapperConnected { get; private set; }

    /// <summary>
    /// Returns the process of the wrapped connection
    /// </summary>
    public string WrappedProcess { get; private set; } = "";

    private NamedPipeServerStream? _ipcPipeStream;
    private NamedPipeServerStream? _auroraInterfacePipeStream;

    private static NamedPipeServerStream CreatePipe(string pipeName)
    {
        var worldSid = new SecurityIdentifier(WellKnownSidType.WorldSid, null);
        var anonymousSid = new SecurityIdentifier(WellKnownSidType.AnonymousSid, null);

        var pipeSecurity = new PipeSecurity();
        pipeSecurity.AddAccessRule(new PipeAccessRule(worldSid,
            PipeAccessRights.ReadWrite | PipeAccessRights.CreateNewInstance,
            AccessControlType.Allow));
        pipeSecurity.AddAccessRule(new PipeAccessRule(anonymousSid,
            PipeAccessRights.ReadWrite | PipeAccessRights.CreateNewInstance,
            AccessControlType.Allow));

        return NamedPipeServerStreamAcl.Create(
            pipeName, PipeDirection.In,
            NamedPipeServerStream.MaxAllowedServerInstances,
            PipeTransmissionMode.Message, PipeOptions.Asynchronous, 5 * 1024, 5 * 1024, pipeSecurity);
    }

    /// <summary>
    /// Starts listening for GameState requests
    /// </summary>
    public bool Start()
    {
        if (_isRunning) return false;
        _isRunning = true;

        BeginIpcServer();
        return true;
    }

    /// <summary>
    /// Stops listening for GameState requests
    /// </summary>
    public async Task Stop()
    {
        _isRunning = false;

        if (_ipcPipeStream != null)
        {
            _ipcPipeStream.Close();
            await _ipcPipeStream.DisposeAsync().AsTask();
            _ipcPipeStream = null;
        }

        if (_auroraInterfacePipeStream != null)
        {
            _auroraInterfacePipeStream.Close();
            await _auroraInterfacePipeStream.DisposeAsync();
            _auroraInterfacePipeStream = null;
        }
    }

    private void BeginIpcServer()
    {
        IsWrapperConnected = false;
        WrappedProcess = "";

        _ipcPipeStream = CreatePipe("Aurora\\server");
        _ipcPipeStream.BeginWaitForConnection(ReceiveGameState, null);
        Global.logger.Information("[IPCServer] Pipe created {PipeName}", _ipcPipeStream?.GetHashCode() ?? -1);

        _auroraInterfacePipeStream = CreatePipe(Constants.AuroraInterfacePipe);
        _auroraInterfacePipeStream.BeginWaitForConnection(ReceiveAuroraCommand, null);
        Global.logger.Information("[AuroraCommandsServerIPC] Pipe created {PipeName}", _auroraInterfacePipeStream?.GetHashCode() ?? -1);
    }

    private void ReceiveGameState(IAsyncResult result)
    {
        if (!_isRunning || _ipcPipeStream == null)
        {
            return;
        }
        Global.logger.Information("[IPCServer] Pipe connection established");

        try
        {
            using var sr = new StreamReader(_ipcPipeStream);
            while (sr.ReadLine() is { } temp)
            {
                try
                {
                    var gameState = JsonSerializer.Deserialize<LfxData>(temp, LfxJsonSourceContext.Default.LfxData) ?? LfxData.Empty;
                    LfxState.SetGameState(gameState);

                    IsWrapperConnected = true;
                    WrappedProcess = LfxState.LastData.Provider.Name.ToLowerInvariant();
                }
                catch (Exception exc)
                {
                    Global.logger.Error(exc, "[IPCServer] ReceiveGameState Exception, ");
                    Global.logger.Information("[IPCServer] Received data that caused error:\n\r{Data}", temp);
                }
            }
        }
        finally
        {
            WrapperConnectionClosed?.Invoke(this, WrappedProcess);
            IsWrapperConnected = false;
            WrappedProcess = "";
        }
        if (!_isRunning)
        {
            return;
        }
        //run in another thread to reset stack
        Task.Run(() =>
        {
            _ipcPipeStream = CreatePipe("Aurora\\server");
            _ipcPipeStream.BeginWaitForConnection(ReceiveGameState, null);
        });
    }

    private void ReceiveAuroraCommand(IAsyncResult result)
    {
        if (!_isRunning || _auroraInterfacePipeStream == null)
        {
            return;
        }
        Global.logger.Information("[AuroraCommandsServerIPC] Pipe connection established");

        using var sr = new StreamReader(_auroraInterfacePipeStream);
        while (sr.ReadLine() is { } command)
        {
            Global.logger.Information("Received command: {Command}", command);
            AuroraCommandReceived?.Invoke(this, command);
        }
        if (!_isRunning)
        {
            return;
        }
        _auroraInterfacePipeStream = CreatePipe(Constants.AuroraInterfacePipe);
        _auroraInterfacePipeStream.BeginWaitForConnection(ReceiveAuroraCommand, null);
    }
}