using System;
using System.Collections.Generic;
using iCUE_ReverseEngineer.Icue.Gsi;

namespace AuroraRgb.Modules.Icue;

public class IcueGsi
{
    public event EventHandler<IcueStateEventArgs>? EventReceived;
    public HashSet<string> States { get; } = [];

    private GsiHandler? _gsiHandler;

    public void SetGsiHandler(GsiHandler gsiHandler)
    {
        _gsiHandler = gsiHandler;
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
        EventReceived?.Invoke(this, icueStateEventArgs);
    }
}