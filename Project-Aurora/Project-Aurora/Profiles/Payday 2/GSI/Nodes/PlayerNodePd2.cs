using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text.Json.Serialization;

namespace AuroraRgb.Profiles.Payday_2.GSI.Nodes;

/// <summary>
/// Information about a player
/// </summary>
[JsonSourceGenerationOptions(PropertyNamingPolicy = JsonKnownNamingPolicy.SnakeCaseLower)]
public class PlayerNodePd2
{
    public static readonly PlayerNodePd2 Default = new();

    /// <summary>
    /// Player name
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Player character
    /// </summary>
    public string Character { get; set; } = string.Empty;

    /// <summary>
    /// Player level
    /// </summary>
    public int Level { get; set; }

    /// <summary>
    /// Player infamy rank
    /// </summary>
    public int Rank { get; set; }

    /// <summary>
    /// Player state
    /// </summary>
    [JsonConverter(typeof(JsonStringEnumConverter<PlayerState>))]
    public PlayerState State { get; set; }

    /// <summary>
    /// Player health information
    /// </summary>
    public HealthNode Health { get; set; } = HealthNode.Default;

    /// <summary>
    /// Player armor information
    /// </summary>
    public ArmorNode Armor { get; set; } = ArmorNode.Default;

    /// <summary>
    /// Player weapons information
    /// </summary>
    public Dictionary<string, WeaponNodePd2> Weapons { get; set; } = [];

    public WeaponNodePd2 SelectedWeapon => Weapons.Values.FirstOrDefault(w => w.IsSelected, WeaponNodePd2.Default);

    /// <summary>
    /// The time left on downed timer
    /// </summary>
    [JsonPropertyName("down_time")]
    public int DownTime { get; set; }

    /// <summary>
    /// The suspicion amount [0.0f - 1.0f]
    /// </summary>
    [JsonPropertyName("suspicion")]
    public double SuspicionAmount { get; set; }

    /// <summary>
    /// The flashed amount [0.0f - 1.0f]
    /// </summary>
    [JsonPropertyName("flashbang_amount")]
    public double FlashAmount { get; set; }

    /// <summary>
    /// A boolean representing if this is the local player
    /// </summary>
    [JsonPropertyName("is_local_player")]
    public bool IsLocalPlayer { get; set; }

    /// <summary>
    /// A boolean representing if this player is in swan song
    /// </summary>
    [JsonPropertyName("is_swansong")]
    public bool SwanSong { get; set; }
}

/// <summary>
/// Information about player's health
/// </summary>
public class HealthNode
{
    public static readonly HealthNode Default = new();

    /// <summary>
    /// Current health amount
    /// </summary>
    public double Current { get; set; }

    /// <summary>
    /// Maximum health amount
    /// </summary>
    [JsonPropertyName("total")]
    public double Max { get; set; }

    /// <summary>
    /// Number of revives left
    /// </summary>
    public int Revives { get; set; }
}

/// <summary>
/// Information about player's armor
/// </summary>
public class ArmorNode
{
    public static readonly ArmorNode Default = new();

    /// <summary>
    /// Maximum amount of armor
    /// </summary>
    public double Max { get; set; }

    /// <summary>
    /// Current amount of armor
    /// </summary>
    public double Current { get; set; }

    /// <summary>
    /// Total amount of armor
    /// </summary>
    public double Total { get; set; }
}

/// <summary>
/// Enum for each player state
/// </summary>
public enum PlayerState
{
    /// <summary>
    /// Undefined
    /// </summary>
    [Description("Undefined")]
    Undefined,

    /// <summary>
    /// Standard state (mask is on)
    /// </summary>
    [Description("Standard")]
    Standard,

    /// <summary>
    /// Mask is off state
    /// </summary>
    [Description("Mask Off")]
    Mask_Off,

    /// <summary>
    /// Player is carrying a bag
    /// </summary>
    [Description("Carrying a bag")]
    Carry,

    /// <summary>
    /// Player is using a bipod
    /// </summary>
    [Description("Using a bipod")]
    Bipod,

    /// <summary>
    /// Player is parachiting, type 1
    /// </summary>
    [Description("Parachuting 1")]
    Jerry1,

    /// <summary>
    /// Player is parachiting, type 2
    /// </summary>
    [Description("Parachuting 2")]
    Jerry2,

    /// <summary>
    /// Player is being tased
    /// </summary>
    [Description("Tased")]
    Tased,

    /// <summary>
    /// Player is clean
    /// </summary>
    [Description("Clean")]
    Clean,

    /// <summary>
    /// Player is a civilian
    /// </summary>
    [Description("Civilian")]
    Civilian,

    /// <summary>
    /// Player is bleeding out
    /// </summary>
    [Description("Bleeding out")]
    Bleed_out,

    /// <summary>
    /// PLayer is completely downed
    /// </summary>
    [Description("Fatal injury")]
    Fatal,

    /// <summary>
    /// Player is incapacitated
    /// </summary>
    [Description("Incapacitated")]
    Incapacitated,

    /// <summary>
    /// Player is in custody
    /// </summary>
    [Description("Arrested")]
    Arrested
}