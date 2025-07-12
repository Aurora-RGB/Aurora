using System;
using System.Collections.Frozen;
using System.Collections.Generic;
using System.Globalization;
using System.Text.Json.Serialization;
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

    FrozenDictionary<string, Func<IGameState, object?>> PropertyMap { get; }
    
    Lazy<ObjectAccessor> LazyObjectAccessor { get; }
}

public abstract class GameState : IGameState
{
    private static LocalPcInformation? _localPcInfo;

    [PublicAPI] // game profiles can still access this
    public static LocalPcInformation LocalPCInfo => _localPcInfo ??= new LocalPcInformation();

    private static AudioNode? _audio;
    [PublicAPI]
    public static AudioNode Audio => _audio ??= new AudioNode();
    
    private static DevicesNode? _devices;
    [PublicAPI]
    public static DevicesNode Devices => _devices ??= new DevicesNode();

    private DesktopNode? _desktop;
    [PublicAPI]
    public DesktopNode Desktop => _desktop ??= new DesktopNode();

    private static MediaNode? _media;
    [PublicAPI]
    public static MediaNode Media => _media ??= new MediaNode();

    private CelestialData? _celestialData;
    public CelestialData CelestialData => _celestialData ??= new CelestialData();
    
    private static ProcessesNode? _processes;
    [PublicAPI]
    public static ProcessesNode Processes => _processes ??= new ProcessesNode();
    
    public Lazy<ObjectAccessor> LazyObjectAccessor { get; }

    [JsonIgnore]
    public virtual FrozenDictionary<string, Func<IGameState, object?>> PropertyMap => FrozenDictionary<string, Func<IGameState, object?>>.Empty;

    /// <summary>
    /// Creates a default GameState instance.
    /// </summary>
    protected GameState()
    {
        LazyObjectAccessor = new Lazy<ObjectAccessor>(() => ObjectAccessor.Create(this));
    }

    #region GameState path resolution
    /// <summary>
    /// Attempts to resolve the given GameState path into a value.<para/>
    /// Returns whether or not the path resulted in a field or property (true) or was invalid (false).
    /// </summary>
    /// <param name="type">The <see cref="GSIPropertyType"/> that the property must match for this to be valid.</param>
    /// <param name="value">The current value of the resulting property or field on this instance.</param>
    private bool TryResolveGsPath(VariablePath path, GSIPropertyType type, out object? value) {
        value = null;
        if (string.IsNullOrEmpty(path.GsiPath)) return false;
        value = this.ResolvePropertyPath(path);
        return value != null && GSIPropertyTypeConverter.IsTypePropertyType(value.GetType(), type);
    }

    public double GetNumber(VariablePath path) {
        //TODO maybe we shouldn't try to parse for every path? It is 1% of the cpu per profiling
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
}

// WIP to be used for gso nodes with System.Text.Json parsing

/// <summary>
/// An empty gamestate with no child nodes.
/// </summary>
public partial class NewtonsoftGameState : GameState
{
    // Holds a cache of the child nodes on this gamestate
    private readonly Dictionary<string, object> _childNodes = new(StringComparer.OrdinalIgnoreCase);
    private readonly Lazy<JObject> _parsedData;
    
    [GameStateIgnore]
    public JObject ParsedData => _parsedData.Value;
    [GameStateIgnore]
    public string Json { get; }
    
    /// <summary>
    /// Should this event be published to other profiles
    /// </summary>
    [GameStateIgnore]
    public bool Announce { get; } = true;

    public NewtonsoftGameState()
    {
        Json = "{}";
        _parsedData = new(() => new JObject());
    }

    public NewtonsoftGameState(string json, bool announce) : this(json)
    {
        Announce = announce;
    }

    public NewtonsoftGameState(string json)
    {
        if (string.IsNullOrWhiteSpace(json))
            json = "{}";

        Json = json;
        _parsedData = new(() =>JObject.Parse(json));
    }

    /// <summary>
    /// Gets the JSON for a child node in this GameState.
    /// </summary>
    public string GetNode(string path) =>
        ParsedData.TryGetValue(path, StringComparison.OrdinalIgnoreCase, out var value) ? value.ToString() : "";

    /// <summary>
    /// Use this method to more-easily lazily return the child node of the given name that exists on this AutoNode.
    /// </summary>
    protected TNode NodeFor<TNode>(string name) where TNode : Node
        => (TNode)(_childNodes.TryGetValue(name, out var n) ? n : _childNodes[name] = Instantiator<TNode, string>.Create(ParsedData[name]?.ToString() ?? ""));

    /// <summary>
    /// Displays the JSON, representative of the GameState data
    /// </summary>
    /// <returns>JSON String</returns>
    public override string ToString() => Json;
}

/// <summary>
/// Attribute that can be applied to properties to indicate they should be excluded from the game state.
/// </summary>
[AttributeUsage(AttributeTargets.Property)]
public class GameStateIgnoreAttribute : Attribute;

[AttributeUsage(AttributeTargets.Class)]
public class GameStateDescriptionAttribute : Attribute
{
    /// <summary>
    /// The description of the game state property.
    /// </summary>
    public string Description { get; }

    /// <summary>
    /// Creates a new GameStateDescriptionAttribute with the given description.
    /// </summary>
    public GameStateDescriptionAttribute(string description)
    {
        Description = description;
    }
}

/// <summary>
/// Attribute that indicates the range of indicies that are valid for an enumerable game state property.
/// </summary>
[AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
public class RangeAttribute(int start, int end) : Attribute
{
    public int Start { get; set; } = start;
    public int End { get; set; } = end;
}