using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Globalization;
using System.Text.Json.Serialization.Metadata;
using AuroraRgb.Nodes;
using AuroraRgb.Utils;
using FastMember;
using JetBrains.Annotations;
using Newtonsoft.Json.Linq;

namespace AuroraRgb.Profiles;

/// <summary>
/// A class representing various information retaining to the game.
/// </summary>
public interface IGameState {
    string Json { get; }        
    string GetNode(string name);

    /// <summary>Attempts to resolve the given path into a numeric value. Returns 0 on failure.</summary>
    double GetNumber(VariablePath path);

    /// <summary>Attempts to resolve the given path into a boolean value. Returns false on failure.</summary>
    bool GetBool(VariablePath path);

    /// <summary>Attempts to resolve the given path into a string value. Returns an empty string on failure.</summary>
    string GetString(VariablePath path);

    /// <summary>Attempts to resolve the given path into a enum value. Returns null on failure.</summary>
    Enum GetEnum(VariablePath path);

    /// <summary>Attempts to resolve the given path into a numeric value. Returns default on failure.</summary>
    TEnum GetEnum<TEnum>(VariablePath path) where TEnum : Enum;

    IReadOnlyDictionary<string, Func<object, object?>> PropertyMap { get; }
    
    Lazy<ObjectAccessor> LazyObjectAccessor { get; }
}

public abstract class GameState : IGameState
{
    private static LocalPcInformation? _localPcInfo;

    // Holds a cache of the child nodes on this gamestate
    private readonly Dictionary<string, object> _childNodes = new(StringComparer.OrdinalIgnoreCase);

    [GameStateIgnore] public JObject _ParsedData { get; }
    [GameStateIgnore] public string Json { get; }

    [PublicAPI] // game profiles can still access this
    public LocalPcInformation LocalPCInfo => _localPcInfo ??= new LocalPcInformation();
    
    public Lazy<ObjectAccessor> LazyObjectAccessor { get; }

    public virtual IReadOnlyDictionary<string, Func<object, object?>> PropertyMap => ImmutableDictionary<string, Func<object, object?>>.Empty;

    /// <summary>
    /// Creates a default GameState instance.
    /// </summary>
    public GameState()
    {
        Json = "{}";
        _ParsedData = new JObject();
        LazyObjectAccessor = new Lazy<ObjectAccessor>(() => ObjectAccessor.Create(this));
    }

    /// <summary>
    /// Creates a GameState instance based on the passed json data.
    /// </summary>
    /// <param name="jsonData">The passed json data</param>
    public GameState(string jsonData)
    {
        if (string.IsNullOrWhiteSpace(jsonData))
            jsonData = "{}";

        Json = jsonData;
        _ParsedData = JObject.Parse(jsonData);
        LazyObjectAccessor = new Lazy<ObjectAccessor>(() => ObjectAccessor.Create(this));
    }

    /// <summary>
    /// Gets the JSON for a child node in this GameState.
    /// </summary>
    public string GetNode(string path) =>
        _ParsedData.TryGetValue(path, StringComparison.OrdinalIgnoreCase, out var value) ? value.ToString() : "";

    /// <summary>
    /// Use this method to more-easily lazily return the child node of the given name that exists on this AutoNode.
    /// </summary>
    protected TNode NodeFor<TNode>(string name) where TNode : Node
        => (TNode)(_childNodes.TryGetValue(name, out var n) ? n : _childNodes[name] = Instantiator<TNode, string>.Create(_ParsedData[name]?.ToString() ?? ""));

    #region GameState path resolution
    /// <summary>
    /// Attempts to resolve the given GameState path into a value.<para/>
    /// Returns whether or not the path resulted in a field or property (true) or was invalid (false).
    /// </summary>
    /// <param name="type">The <see cref="GSIPropertyType"/> that the property must match for this to be valid.</param>
    /// <param name="value">The current value of the resulting property or field on this instance.</param>
    private bool TryResolveGsPath(VariablePath path, GSIPropertyType type, out object? value) {
        value = null;
        return !string.IsNullOrEmpty(path.GsiPath)
               && (value = this.ResolvePropertyPath(path.GsiPath)) != null
               && GSIPropertyTypeConverter.IsTypePropertyType(value?.GetType(), type);
    }

    public double GetNumber(VariablePath path) {
        if (double.TryParse(path.GsiPath, CultureInfo.InvariantCulture, out var val)) // If the path is a raw number, return that
            return val;
        if (TryResolveGsPath(path, GSIPropertyType.Number, out var pVal)) // Next, try resolve the path as we would other types
            return Convert.ToDouble(pVal);
        return 0;
    }

    public bool GetBool(VariablePath path) => TryResolveGsPath(path, GSIPropertyType.Boolean, out var @bool) && Convert.ToBoolean(@bool);
    public string GetString(VariablePath path) => TryResolveGsPath(path, GSIPropertyType.String, out var str) ? str.ToString() : "";
    public Enum GetEnum(VariablePath path) => TryResolveGsPath(path, GSIPropertyType.Enum, out var @enum) && @enum is Enum e ? e : null;
    public TEnum GetEnum<TEnum>(VariablePath path) where TEnum : Enum => TryResolveGsPath(path, GSIPropertyType.Enum, out var @enum) && @enum is TEnum e ? e : default;
    #endregion

    /// <summary>
    /// Displays the JSON, representative of the GameState data
    /// </summary>
    /// <returns>JSON String</returns>
    public override string ToString() => Json;
}

// WIP to be used for gso nodes with System.Text.Json parsing
public class JsonGameState : IGameState
{
    //TODO replace this
    public string Json { get; } = "";

    private static readonly Dictionary<string, Func<object, string>> StringGetters = new();
    private static readonly Dictionary<string, Func<object, double>> NumberGetters = new();
    private static readonly Dictionary<string, Func<object, bool>> BoolGetters = new();
    private static readonly Dictionary<string, Func<object, Enum>> EnumGetters = new();

    public IReadOnlyDictionary<string, Func<object, object?>> PropertyMap { get; } = new Dictionary<string, Func<object, object?>>();
    public Lazy<ObjectAccessor> LazyObjectAccessor { get; }

    public JsonGameState()
    {
        LazyObjectAccessor = new Lazy<ObjectAccessor>(() => ObjectAccessor.Create(this));
    }

    protected static void PrepProps(IEnumerable<JsonPropertyInfo> jsonPropertyInfos, Func<object, object?>? parentGetter = null, string root = "")
    {
        foreach (var jsonPropertyInfo in jsonPropertyInfos)
        {
            var propType = jsonPropertyInfo.PropertyType;
            if (propType.IsEnum)
            {
                EnumGetters[root + propType.Name] = p =>
                {
                    var parent = parentGetter?.Invoke(p) ?? p;
                    return jsonPropertyInfo.Get!.Invoke(parent) as Enum;
                };
            }else if (propType.IsAssignableTo(typeof(string)))
            {
                StringGetters[root + propType.Name] = p =>
                {
                    var parent = parentGetter?.Invoke(p) ?? p;
                    return jsonPropertyInfo.Get!.Invoke(parent) as string ?? string.Empty;
                };
            }else if (propType.IsAssignableTo(typeof(double)))
            {
                NumberGetters[root + propType.Name] = p =>
                {
                    var parent = parentGetter?.Invoke(p) ?? p;
                    var o = jsonPropertyInfo.Get!.Invoke(parent);
                    if (o == null)
                    {
                        return 0.0;
                    }

                    return (double)o;
                };
            }else if (propType.IsAssignableTo(typeof(bool)))
            {
                BoolGetters[root + propType.Name] = p =>
                {
                    var parent = parentGetter?.Invoke(p) ?? p;
                    var o = jsonPropertyInfo.Get!.Invoke(parent);
                    if (o == null)
                    {
                        return false;
                    }

                    return (bool)o;
                };
            }else if (propType.IsClass)
            {
                PrepProps(jsonPropertyInfo.Options.GetTypeInfo(propType).Properties, jsonPropertyInfo.Get, root + VariablePath.Seperator + jsonPropertyInfo.Name);
            }
        }
    }
    public string GetNode(string name)
    {
        return StringGetters[name].Invoke(this);
    }

    public double GetNumber(VariablePath path)
    {
        return NumberGetters[path.GsiPath].Invoke(this);
    }

    public bool GetBool(VariablePath path)
    {
        return BoolGetters[path.GsiPath].Invoke(this);
    }

    public string GetString(VariablePath path)
    {
        return GetNode(path.GsiPath);
    }

    public Enum GetEnum(VariablePath path)
    {
        return EnumGetters[path.GsiPath].Invoke(this);
    }

    public TEnum GetEnum<TEnum>(VariablePath path) where TEnum : Enum
    {
        return (TEnum)GetEnum(path);
    }
}

/// <summary>
/// An empty gamestate with no child nodes.
/// </summary>
public partial class EmptyGameState : GameState
{
    public EmptyGameState() { }
    public EmptyGameState(string json) : base(json) { }
}


/// <summary>
/// Attribute that can be applied to properties to indicate they should be excluded from the game state.
/// </summary>
[AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
public class GameStateIgnoreAttribute : Attribute { }

/// <summary>
/// Attribute that indicates the range of indicies that are valid for an enumerable game state property.
/// </summary>
[AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
public class RangeAttribute : Attribute {

    public RangeAttribute(int start, int end) {
        Start = start;
        End = end;
    }

    public int Start { get; set; }
    public int End { get; set; }
}