using System.Text.Json.Serialization;

namespace AuroraRgb.Profiles.Dota_2.GSI.Nodes;

/// <summary>
/// Class representing item information
/// </summary>
public class Item
{
    public static readonly Item Default = new();

    /// <summary>
    /// Item name
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// The name of the rune cotnained inside this item.
    /// <note type="note">Possible rune names: empty, arcane, bounty, double_damage, haste, illusion, invisibility, regen</note>
    /// </summary>
    public string? ContainsRune { get; set; } = "empty";

    /// <summary>
    /// A boolean representing whether this item can be casted
    /// </summary>
    public bool CanCast { get; set; }

    /// <summary>
    /// Item's cooldown
    /// </summary>
    public int Cooldown { get; set; }

    /// <summary>
    /// A boolean representing whether this item is passive
    /// </summary>
    [JsonPropertyName("passive")]
    public bool IsPassive { get; set; }

    /// <summary>
    /// The amount of charges on this item
    /// </summary>
    public int Charges { get; set; }
}