﻿using System.Text.Json;
using System.Text.Json.Serialization;
using Common.Utils;

namespace Common.Devices;

[Serializable]
public class VariableRegistryItem
{
    public object? Value;
    private object? @default;

    [JsonIgnore]
    public object? Default
    {
        get => @default?.TryClone();
        private set => @default = value?.TryClone();
    }

    [JsonIgnore]
    public object? Max;
    [JsonIgnore]
    public object? Min;
    [JsonIgnore]
    public string Title = "";
    [JsonIgnore]
    public string Remark = "";
    [JsonIgnore]
    public VariableFlags Flags = VariableFlags.None;

    public VariableRegistryItem()
    {
    }

    public VariableRegistryItem(object? defaultValue, object? max = null, object? min = null, string title = "", string remark = "",
        VariableFlags flags = VariableFlags.None)
    {
        Value = defaultValue;
        Default = defaultValue;

        if (Value != null && max != null && Value.GetType() == max.GetType())
            Max = max;

        if (Value != null && min != null && Value.GetType() == min.GetType())
            Min = min;

        Title = title;
        Remark = remark;
        Flags = flags;
    }

    public VariableRegistryItem(object? value, object? defaultValue, object? max = null, object? min = null, string title = "",
        string remark = "", VariableFlags flags = VariableFlags.None)
    {
        Value = value ?? defaultValue;
        Default = defaultValue;

        if (Value != null && max != null && Value.GetType() == max.GetType())
            Max = max;

        if (Value != null && min != null && Value.GetType() == min.GetType())
            Min = min;

        Title = title;
        Remark = remark;
        Flags = flags;
    }

    public void SetVariable(object? newvalue)
    {
        if (Value != null && newvalue != null && Value.GetType() == newvalue.GetType())
        {
            Value = newvalue;
        }
    }

    internal void Merge(VariableRegistryItem variableRegistryItem)
    {
        Default = variableRegistryItem.Default;
        Title = variableRegistryItem.Title;
        Remark = variableRegistryItem.Remark;
        Min = variableRegistryItem.Min;
        Max = variableRegistryItem.Max;
        var typ = Value.GetType();
        if (variableRegistryItem.Default != null)
        {
            var defaultType = variableRegistryItem.Default.GetType();

            if (defaultType != typ && typ == typeof(long) && defaultType.IsEnum)
                Value = Enum.ToObject(defaultType, Value);
            else if (defaultType != typ && Value is long && TypeUtils.IsNumericType(defaultType))
                Value = Convert.ChangeType(Value, defaultType);
            else if (Value == null && defaultType != typ)
                Value = variableRegistryItem.Default;
        }

        Flags = variableRegistryItem.Flags;
    }
}

public enum VariableFlags
{
    None = 0,
    UseHEX = 1
}

public class VariableRegistry : ICloneable //Might want to implement something like IEnumerable here
{
    [JsonPropertyName("Variables")]
    private Dictionary<string, VariableRegistryItem> _variables = new();

    [JsonIgnore]
    public int Count => _variables.Count;

    [JsonIgnore]
    public IEnumerable<(string, VariableRegistryItem)> Variables => _variables.Select(kv => (kv.Key, kv.Value));

    public void Combine(VariableRegistry otherRegistry, bool removeMissing = false)
    {
        //Below doesn't work for added variables
        var vars = new Dictionary<string, VariableRegistryItem>();

        foreach (var variable in otherRegistry._variables)
        {
            if (removeMissing)
            {
                var local = _variables.ContainsKey(variable.Key) ? _variables[variable.Key] : null;
                if (local != null)
                    local.Merge(variable.Value);
                else
                    local = variable.Value;

                vars.Add(variable.Key, local);
            }
            else
                Register(variable.Key, variable.Value);
        }

        if (removeMissing)
            _variables = vars;
    }

    public IEnumerable<string> GetRegisteredVariableKeys()
    {
        return _variables.Keys.ToArray();
    }

    public void Register(string name, object defaultValue, string title = "", object max = null, object min = null, string remark = "",
        VariableFlags flags = VariableFlags.None)
    {
        if (!_variables.ContainsKey(name))
            _variables.Add(name, new VariableRegistryItem(defaultValue, max, min, title, remark, flags));
    }

    public void Register(string name, VariableRegistryItem varItem)
    {
        if (!_variables.ContainsKey(name))
            _variables.Add(name, varItem);
        else
            _variables[name].Merge(varItem);
    }

    public bool SetVariable(string name, object? variable)
    {
        if (_variables.ContainsKey(name))
        {
            _variables[name].SetVariable(variable);
            return true;
        }

        return false;
    }

    public void ResetVariable(string name)
    {
        if (_variables.ContainsKey(name))
        {
            _variables[name].Value = _variables[name].Default;
        }
    }

    public T GetVariable<T>(string name)
    {
        if (_variables.ContainsKey(name) && _variables[name] != null && _variables[name].Value != null &&
            typeof(T).IsAssignableFrom(_variables[name].Value.GetType()))
            return (T)_variables[name].Value;

        return default(T);
    }

    public bool GetVariableMax<T>(string name, out T value)
    {
        if (_variables.ContainsKey(name) && _variables[name] != null && _variables[name].Max != null && _variables[name].Value is T)
        {
            value = (T)_variables[name].Max;
            return true;
        }

        value = Activator.CreateInstance<T>();
        return false;
    }

    public bool GetVariableMin<T>(string name, out T value)
    {
        if (_variables.ContainsKey(name) && _variables[name] != null && _variables[name].Min != null && _variables[name].Value is T)
        {
            value = (T)_variables[name].Min;
            return true;
        }

        value = Activator.CreateInstance<T>();
        return false;
    }

    public Type GetVariableType(string name)
    {
        if (_variables.ContainsKey(name) && _variables[name] != null && _variables[name].Value != null)
            return _variables[name].Value.GetType();

        return typeof(object);
    }

    public string GetTitle(string name)
    {
        if (_variables.TryGetValue(name, out var variable))
            return variable.Title;

        return "";
    }

    public string GetRemark(string name)
    {
        return _variables.TryGetValue(name, out var variable) ? variable.Remark : "";
    }

    public VariableFlags GetFlags(string name)
    {
        return _variables.TryGetValue(name, out var variable) ? variable.Flags : VariableFlags.None;
    }

    public object Clone()
    {
        var str = JsonSerializer.Serialize(this, new JsonSerializerOptions{ WriteIndented = false});

        return JsonSerializer.Deserialize(
            str,
            GetType()
        )!;
    }
}