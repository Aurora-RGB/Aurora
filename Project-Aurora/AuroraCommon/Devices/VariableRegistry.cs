using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;
using Common.Utils;

namespace Common.Devices;

[Serializable]
public class VariableRegistryItem : ICloneable
{
    public object? Value { get; private set; }

    public object Default { get; private set; }

    public object? Max { get; private set; }
    public object? Min { get; private set; }
    public string Title { get; private set; }
    public string Remark { get; private set; }
    public IntVariableDisplay Flags { get; private set; }

    [JsonConstructor]
    public VariableRegistryItem(object? value, object @default, object? max = null, object? min = null, string title = "",
        string remark = "", IntVariableDisplay flags = IntVariableDisplay.None)
    {
        value = UnwrapJsonNode(value);
        @default = UnwrapJsonNode(@default);
        max = UnwrapJsonNode(max);
        min = UnwrapJsonNode(min);
        
        Value = value ?? @default;
        Default = @default;

        if (Value != null && max != null && Value.GetType() == max.GetType())
            Max = max;

        if (Value != null && min != null && Value.GetType() == min.GetType())
            Min = min;

        Title = title;
        Remark = remark;
        Flags = flags;
    }

    [return: NotNullIfNotNull("value")]
    private static object? UnwrapJsonNode(object? value)
    {
        if (value is JsonElement jsonElement)
        {
            value = jsonElement.ValueKind switch
            {
                JsonValueKind.Null => null,
                JsonValueKind.Number => GetNumber(jsonElement),
                JsonValueKind.String => GetStringOrColor(jsonElement),
                JsonValueKind.True => true,
                JsonValueKind.False => false,
                JsonValueKind.Object => GetColor(jsonElement),
                _ => throw new JsonException("Unexpected VariableRegistryItem type: " + jsonElement.ValueKind),
            };
        }

        return value;
    }

    private static object GetColor(JsonElement jsonElement)
    {
        return jsonElement.Deserialize<SimpleColor>();
    }

    private static object? GetStringOrColor(JsonElement jsonElement)
    {
        var str = jsonElement.GetString();
        var args = str?.Split(',');

        if (args == null)
        {
            return null;
        }
        
        var colors = args.Select(byte.Parse);
        return args.Length switch
        {
            3 => FromRgb(),
            4 => FromArgb(),
            _ => str
        };

        SimpleColor FromRgb()
        {
            var rgb = colors.ToArray();
            return SimpleColor.FromRgba(rgb[0], rgb[1], rgb[2]);
        }
        SimpleColor FromArgb()
        {
            var argb = colors.ToArray();
            return SimpleColor.FromRgba(argb[1], argb[2], argb[3], argb[0]);
        }
    }

    private static object GetNumber(JsonElement jsonElement)
    {
        if (int.TryParse(jsonElement.GetDecimal().ToString(CultureInfo.InvariantCulture), CultureInfo.InvariantCulture, out var intVal))
            return intVal;
        
        return jsonElement.GetDouble();
    }

    public void SetVariable(object? newValue)
    {
        if (Value != null && newValue != null && Value.GetType() == newValue.GetType())
        {
            Value = newValue;
        }
    }

    internal void Merge(VariableRegistryItem variableRegistryItem)
    {
        Default = variableRegistryItem.Default;
        Title = variableRegistryItem.Title;
        Remark = variableRegistryItem.Remark;
        Min = variableRegistryItem.Min;
        Max = variableRegistryItem.Max;

        Flags = variableRegistryItem.Flags;

        var defaultType = variableRegistryItem.Default.GetType();
        var typ = (Value ?? Default).GetType();

        if (defaultType != typ && TypeUtils.IsNumericType(typ) && defaultType.IsEnum)
            Value = Enum.ToObject(defaultType, Value ?? Default);
        else if (defaultType != typ && Value is long && TypeUtils.IsNumericType(defaultType))
            Value = Convert.ChangeType(Value, defaultType);
        else if (Value == null && defaultType != typ)
            Value = variableRegistryItem.Default;
        else if (defaultType != typ)
            Value = Default;
    }

    public object Clone()
    {
        return new VariableRegistryItem(Value?.TryClone(), Default.TryClone(), Max, Min, Title, Remark);
    }

    public void Reset()
    {
        Value = Default;
    }
}

public enum IntVariableDisplay
{
    None = 0,
    UseHex = 1
}

public class VariableRegistry : ICloneable //Might want to implement something like IEnumerable here
{
    [JsonIgnore]
    public int Count => Variables.Count;

    public IDictionary<string, VariableRegistryItem> Variables { get; set; } = new Dictionary<string, VariableRegistryItem>();

    public void Combine(VariableRegistry otherRegistry, bool removeMissing = false)
    {
        //Below doesn't work for added variables
        var vars = new Dictionary<string, VariableRegistryItem>();

        foreach (var variable in otherRegistry.Variables)
        {
            if (removeMissing)
            {
                var local = Variables.TryGetValue(variable.Key, out var outVar) ? outVar : null;
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
            Variables = vars;
    }

    public IEnumerable<string> GetRegisteredVariableKeys()
    {
        return Variables.Keys.ToArray();
    }

    public void Register(string name, object defaultValue, string title = "", object? max = null, object? min = null, string remark = "",
        IntVariableDisplay flags = IntVariableDisplay.None)
    {
        if (!Variables.ContainsKey(name))
            Variables.Add(name, new VariableRegistryItem(null, defaultValue, max, min, title, remark, flags));
    }

    public void Register(string name, VariableRegistryItem varItem)
    {
        if (!Variables.TryAdd(name, varItem))
            Variables[name].Merge(varItem);
    }

    public void SetVariable(string name, object? variable)
    {
        if (!Variables.ContainsKey(name)) return;

        Variables[name].SetVariable(variable);
    }

    public void ResetVariable(string name)
    {
        if (Variables.TryGetValue(name, out var variable))
        {
            variable.Reset();
        }
    }

    public string GetString(string name)
    {
        if (Variables.TryGetValue(name, out var value) && value.Value is string strVal)
            return strVal;
        
        return string.Empty;
    }

    public T GetVariable<T>(string name) where T : new()
    {
        if (Variables.TryGetValue(name, out var value))
            return (T)value.Value!;

        return default(T) ?? new T();
    }

    public bool GetVariableMax<T>(string name, out T? value)
    {
        if (Variables.TryGetValue(name, out var outVal) && outVal is { Max: not null, Value: T })
        {
            value = (T)outVal.Max;
            return true;
        }

        value = Activator.CreateInstance<T>();
        return false;
    }

    public bool GetVariableMin<T>(string name, out T value)
    {
        if (Variables.TryGetValue(name, out var outVal) && outVal is { Min: not null, Value: T })
        {
            value = (T)outVal.Min;
            return true;
        }

        value = Activator.CreateInstance<T>();
        return false;
    }

    public Type GetVariableType(string name)
    {
        if (Variables.TryGetValue(name, out var value) && value.Value != null)
            return value.Value.GetType();

        return typeof(object);
    }

    public string GetTitle(string name)
    {
        return Variables.TryGetValue(name, out var variable) ? variable.Title : string.Empty;
    }

    public string GetRemark(string name)
    {
        return Variables.TryGetValue(name, out var variable) ? variable.Remark : string.Empty;
    }

    public IntVariableDisplay GetFlags(string name)
    {
        return Variables.TryGetValue(name, out var variable) ? variable.Flags : IntVariableDisplay.None;
    }

    public object Clone()
    {
        var clone = new VariableRegistry();
        foreach (var registryItem in Variables)
        {
            clone.Variables[registryItem.Key] = (VariableRegistryItem)registryItem.Value.Clone();
        }
        return clone;
    }
}