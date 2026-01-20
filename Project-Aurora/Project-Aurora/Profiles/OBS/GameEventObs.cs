using System;
using System.Threading.Tasks;
using AuroraRgb.Profiles.Generic;
using OBSWebsocketDotNet;
using OBSWebsocketDotNet.Communication;
using OBSWebsocketDotNet.Types;
using OBSWebsocketDotNet.Types.Events;

namespace AuroraRgb.Profiles.OBS;

public sealed class GameEventObs : GameEvent_Generic
{
    private readonly OBSWebsocket _obsWebsocket = new();

    private bool _connecting = true;
    private int _connectionAttempts;

    public override void OnStart()
    {
        base.OnStart();

        _obsWebsocket.Connected += ObsWebsocketOnConnected;
        _obsWebsocket.Disconnected += ObsWebsocketOnDisconnected;
        _obsWebsocket.UnsupportedEvent += ObsWebsocketOnUnsupportedEvent;
        _obsWebsocket.RecordStateChanged += ObsWebsocketOnRecordStateChanged;
        _obsWebsocket.StreamStateChanged += ObsWebsocketOnStreamStateChanged;
        _obsWebsocket.ConnectAsync(Global.Configuration.ObsWebsocketUrl, Global.SensitiveData.ObsWebSocketPassword);
    }

    public override void OnStop()
    {
        base.OnStop();

        _connecting = false;
        _obsWebsocket.Disconnect();
    }

    private void ObsWebsocketOnConnected(object? sender, EventArgs e)
    {
        Global.logger.Information("OBS WebSocket connected");
        if (GameState is not GameStateObs gameState) return;
        gameState.IsConnected = true;
        gameState.IsRecording = _obsWebsocket.GetRecordStatus().IsRecording;
        gameState.IsStreaming = _obsWebsocket.GetStreamStatus().IsActive;
    }

    private static void ObsWebsocketOnUnsupportedEvent(object? sender, UnsupportedEventArgs e)
    {
        Global.logger.Warning("OBS WebSocket received unsupported event: {EventName}", e.EventType);
    }

    private void ObsWebsocketOnDisconnected(object? sender, ObsDisconnectionInfo e)
    {
        Global.logger.Information("OBS WebSocket disconnected: {Reason}", e.DisconnectReason);
        if (GameState is not GameStateObs gameState) return;
        gameState.IsConnected = false;
        gameState.IsRecording = false;
        gameState.IsStreaming = false;

        if (!_connecting || _connectionAttempts >= 5) return;
        Global.logger.Information("Attempting to reconnect to OBS WebSocket (Attempt {Attempt})", _connectionAttempts++);
        Task.Delay(2000).ContinueWith(_ =>
            _obsWebsocket.ConnectAsync(Global.Configuration.ObsWebsocketUrl, Global.SensitiveData.ObsWebSocketPassword)
        );
    }

    private void ObsWebsocketOnRecordStateChanged(object? sender, RecordStateChangedEventArgs e)
    {
        if (GameState is not GameStateObs gameState)
        {
            return;
        }

        gameState.IsRecording = e.OutputState.State == OutputState.OBS_WEBSOCKET_OUTPUT_STARTED;
    }

    private void ObsWebsocketOnStreamStateChanged(object? sender, StreamStateChangedEventArgs e)
    {
        if (GameState is not GameStateObs gameState)
        {
            return;
        }

        gameState.IsStreaming = e.OutputState.State == OutputState.OBS_WEBSOCKET_OUTPUT_STARTED;
    }

    public Task<string> ReconnectWebSocket()
    {
        _obsWebsocket.Disconnect();
        _connecting = true;
        _connectionAttempts = 0;
        var tcs = new TaskCompletionSource<string>();
        _obsWebsocket.Connected += SetConnected;
        _obsWebsocket.Disconnected += SetDisconnected;

        _obsWebsocket.ConnectAsync(Global.Configuration.ObsWebsocketUrl, Global.SensitiveData.ObsWebSocketPassword);
        return tcs.Task;

        void SetConnected(object? sender, EventArgs e)
        {
            tcs.TrySetResult("Websocket connected successfully");

            _obsWebsocket.Connected -= SetConnected;
            _obsWebsocket.Disconnected -= SetDisconnected;
        }

        void SetDisconnected(object? sender, ObsDisconnectionInfo e)
        {
            tcs.TrySetResult($"OBS WebSocket connection failed:\n{e.DisconnectReason}");

            _obsWebsocket.Connected -= SetConnected;
            _obsWebsocket.Disconnected -= SetDisconnected;
        }
    }
}