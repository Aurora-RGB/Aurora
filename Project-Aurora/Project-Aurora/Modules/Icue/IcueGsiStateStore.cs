using System;
using System.Collections.Generic;

namespace AuroraRgb.Modules.Icue;

public sealed class IcueGsiStateStore(string gameName)
{
    private readonly HashSet<string> _states = [];
    private readonly HashSet<string> _events = [];

    public event EventHandler? StateChanged;

    public string GameName { get; } = gameName;

    public IReadOnlySet<string> States => _states;

    public IReadOnlySet<string> Events => _events;

    public void AddState(string state)
    {
        if (_states.Add(state))
            StateChanged?.Invoke(this, EventArgs.Empty);
    }
    
    public void AddEvent(string state)
    {
        if (_events.Add(state))
            StateChanged?.Invoke(this, EventArgs.Empty);
    }
}