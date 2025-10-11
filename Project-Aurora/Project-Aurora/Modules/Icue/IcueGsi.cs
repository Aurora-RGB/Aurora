using System;
using System.Collections.Generic;
using iCUE_ReverseEngineer.Icue.Gsi;

namespace AuroraRgb.Modules.Icue;

public class IcueGsi
{
    public event EventHandler<EventArgs>? GameChanged;
    public event EventHandler<IcueStateEventArgs>? EventReceived;
    public HashSet<string> States { get; } = [];
    public Dictionary<string, long> EventTimestamps { get; } = [];
    public string GameName { get; private set; } = string.Empty;

    private GsiHandler? _gsiHandler;

    public void SetGsiHandler(GsiHandler gsiHandler, IcueGsiConnectionEventArgs e)
    {
        _gsiHandler = gsiHandler;
        _gsiHandler.StateAdded += OnStateAdded;
        _gsiHandler.StateRemoved += OnStateRemoved;
        _gsiHandler.StatesCleared += OnStatesCleared;
        _gsiHandler.EventAdded += OnEventAdded;

        GameName = e.GameName;
        GameChanged?.Invoke(this, EventArgs.Empty);
    }

    public void ClearGsiHandler()
    {
        if (_gsiHandler == null)
        {
            return;
        }
        
        GameName = string.Empty;
        GameChanged?.Invoke(this, EventArgs.Empty);

        _gsiHandler.StateAdded -= OnStateAdded;
        _gsiHandler.StateRemoved -= OnStateRemoved;
        _gsiHandler.StatesCleared -= OnStatesCleared;
        _gsiHandler.EventAdded -= OnEventAdded;

        _gsiHandler = null;
    }

    private void OnStateAdded(object? sender, IcueStateEventArgs icueStateEventArgs)
    {
        States.Add(icueStateEventArgs.StateName);
    }

    private void OnStateRemoved(object? sender, IcueStateEventArgs icueStateEventArgs)
    {
        States.Remove(icueStateEventArgs.StateName);
    }

    private void OnStatesCleared(object? sender, EventArgs e)
    {
        States.Clear();
    }

    private void OnEventAdded(object? sender, IcueStateEventArgs icueStateEventArgs)
    {
        EventTimestamps[icueStateEventArgs.StateName] = DateTimeOffset.Now.ToUnixTimeMilliseconds();
        EventReceived?.Invoke(this, icueStateEventArgs);
    }
}