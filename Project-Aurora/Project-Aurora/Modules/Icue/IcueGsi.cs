using System;
using System.Collections.Generic;
using iCUE_ReverseEngineer.Icue.Gsi;

namespace AuroraRgb.Modules.Icue;

public class IcueGsi
{
    public event EventHandler<IcueGsiConnectionEventArgs>? GameChanged;
    public event EventHandler<IcueStateEventArgs>? EventReceived;
    public HashSet<string> States { get; } = [];
    public string GameName { get; private set; } = string.Empty;

    private GsiHandler? _gsiHandler;

    public void SetGsiHandler(GsiHandler gsiHandler)
    {
        _gsiHandler = gsiHandler;
        _gsiHandler.GameConnected += GsiHandlerOnGameConnected;
        _gsiHandler.StateAdded += OnStateAdded;
        _gsiHandler.StateRemoved += OnStateRemoved;
        _gsiHandler.StatesCleared += OnStatesCleared;
        _gsiHandler.EventAdded += OnEventAdded;
    }

    public void ClearGsiHandler()
    {
        if (_gsiHandler == null)
        {
            return;
        }
        
        GameName = string.Empty;

        _gsiHandler.StateAdded -= OnStateAdded;
        _gsiHandler.StateRemoved -= OnStateRemoved;
        _gsiHandler.StatesCleared -= OnStatesCleared;
        _gsiHandler.EventAdded -= OnEventAdded;

        _gsiHandler = null;
    }

    private void GsiHandlerOnGameConnected(object? sender, IcueGsiConnectionEventArgs e)
    {
        GameName = e.GameName;
        GameChanged?.Invoke(this, e);
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
        EventReceived?.Invoke(this, icueStateEventArgs);
    }
}