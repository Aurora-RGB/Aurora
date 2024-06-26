﻿using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using AuroraRgb.Modules;

namespace AuroraRgb.Settings;

public class Keybind
{
    [Newtonsoft.Json.JsonProperty("_AssignedKeys")]
    private List<Keys> _assignedKeys = [];

    public Keybind()
    {
    }

    public Keybind(IEnumerable<Keys> keys)
    {
        _assignedKeys = [..keys];
    }

    public void SetKeys(IEnumerable<Keys> keys)
    {
        _assignedKeys = [..keys];
    }

    private bool IsEmpty()
    {
        return _assignedKeys.Count == 0;
    }

    public bool IsPressed()
    {
        var pressedKeys = InputsModule.InputEvents.Result.PressedKeys;

        return pressedKeys.Count > 0 && pressedKeys.SequenceEqual(_assignedKeys);
    }

    public Keys[] ToArray()
    {
        return _assignedKeys.ToArray();
    }

    public override string ToString()
    {
        if (IsEmpty())
            return "[EMPTY]";

        var sb = new StringBuilder();
        var keysCopy = new Queue<Keys>(_assignedKeys);

        while (keysCopy.Count > 0)
        {
            var key = keysCopy.Dequeue();

            sb.Append(key.ToString());

            if (keysCopy.Count > 0)
                sb.Append(" + ");
        }

        return sb.ToString();
    }

    public Keybind Clone()
    {
        return new Keybind(_assignedKeys.ToArray());
    }
}