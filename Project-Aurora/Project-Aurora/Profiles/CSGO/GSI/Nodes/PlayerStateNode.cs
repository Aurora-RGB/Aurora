using System.Text.Json.Serialization;

namespace AuroraRgb.Profiles.CSGO.GSI.Nodes;

/// <summary>
/// Class representing various player states
/// </summary>
public class PlayerStateNode
{
    public static readonly PlayerStateNode Default = new();
        
    /// <summary>
    /// Player's health
    /// </summary>
    public int Health { get; set; }

    /// <summary>
    /// Player's armor
    /// </summary>
    public int Armor { get; set; }

    /// <summary>
    /// Boolean representing whether or not the player has a helmet
    /// </summary>
    [JsonPropertyName("helmet")]
    public bool Helmet { get; set; }

    /// <summary>
    /// Player's flash amount
    /// </summary>
    public int Flashed { get; set; }

    /// <summary>
    /// Player's smoked amount
    /// </summary>
    public int Smoked { get; set; }

    /// <summary>
    /// Player's burning amount
    /// </summary>
    public int Burning { get; set; }

    /// <summary>
    /// Player's current money
    /// </summary>
    public int Money { get; set; }

    /// <summary>
    /// Player's current round kills
    /// </summary>
    public int RoundKills { get; set; }

    /// <summary>
    /// Player's current round kills (headshots only)
    /// </summary>
    [JsonPropertyName("round_killhs")]
    public int RoundKillHS { get; set; }

    /// <summary>
    /// Value of equipment
    /// </summary>
    public int EquipValue { get; set; }
}