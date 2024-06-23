using System.Text.Json.Serialization;

namespace AuroraRgb.Profiles.Dota_2.GSI.Nodes;

/// <summary>
/// Class representing ability information
/// </summary>
public class Ability
{
    public static readonly Ability Default = new();

    /// <summary>
    /// Ability name
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Ability level
    /// </summary>
    public int Level { get; set; }

    /// <summary>
    /// A boolean representing whether the ability can be casted
    /// </summary>
    [JsonPropertyName("can_cast")]
    public bool CanCast { get; set; }

    /// <summary>
    /// A boolean representing whether the ability is passive
    /// </summary>
    [JsonPropertyName("passive")]
    public bool IsPassive { get; set; }

    /// <summary>
    /// A boolean representing whether the ability is active
    /// </summary>
    [JsonPropertyName("ability_active")]
    public bool IsActive { get; set; }

    /// <summary>
    /// Ability cooldown
    /// </summary>
    public int Cooldown { get; set; }

    /// <summary>
    /// A boolean representing whether the ability is an ultimate
    /// </summary>
    [JsonPropertyName("ultimate")]
    public bool IsUltimate { get; set; }
}