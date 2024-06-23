using System.ComponentModel;
using System.Text.Json.Serialization;

namespace AuroraRgb.Profiles.CSGO.GSI.Nodes;

/// <summary>
/// A class representing weapon information
/// </summary>
public class WeaponNode
{
    public static readonly WeaponNode Default = new();

    /// <summary>
    /// Weapon's name
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Weapon's skin name
    /// </summary>
    public string Paintkit { get; set; } = string.Empty;

    /// <summary>
    /// Weapon type
    /// </summary>
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public WeaponType Type { get; set; }

    /// <summary>
    /// Curren amount of ammo in the clip
    /// </summary>
    public int AmmoClip { get; set; }

    /// <summary>
    /// The maximum amount of ammo in the clip
    /// </summary>
    public int AmmoClipMax { get; set; }

    /// <summary>
    /// The amount of ammo in reserve
    /// </summary>
    public int AmmoReserve { get; set; }

    /// <summary>
    /// Weapon's state
    /// </summary>
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public WeaponState State { get; set; }
}

/// <summary>
/// Enum list for all types of weapons
/// </summary>
public enum WeaponType
{
    /// <summary>
    /// Undefined
    /// </summary>
    Undefined,

    /// <summary>
    /// Rifle
    /// </summary>
    Rifle,

    /// <summary>
    /// Sniper rifles
    /// </summary>
    [Description("Sniper Rifle")]
    SniperRifle,

    /// <summary>
    /// Submachine gun
    /// </summary>
    [Description("Submachine Gun")]
    SubmachineGun,

    /// <summary>
    /// Shotgun
    /// </summary>
    Shotgun,

    /// <summary>
    /// Machine gun
    /// </summary>
    [Description("Machine Gun")]
    MachineGun,

    /// <summary>
    /// Pistol
    /// </summary>
    Pistol,

    /// <summary>
    /// Knife
    /// </summary>
    Knife,

    /// <summary>
    /// Grenade
    /// </summary>
    Grenade,

    /// <summary>
    /// C4
    /// </summary>
    C4,

    /// <summary>
    /// Stackable Item
    /// </summary>
    [Description("Stackable Item")]
    StackableItem
}

/// <summary>
/// Enum list for all weapon states
/// </summary>
public enum WeaponState
{
    /// <summary>
    /// Undefined
    /// </summary>
    Undefined,

    /// <summary>
    /// Active (in hand)
    /// </summary>
    Active,

    /// <summary>
    /// Holstered
    /// </summary>
    Holstered,

    /// <summary>
    /// Reloading
    /// </summary>
    Reloading
}