using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace AuroraRgb.Profiles.CSGO.GSI.Nodes;

/// <summary>
/// Class representing player information
/// </summary>
public class PlayerNode
{
    public static readonly PlayerNode Default = new();

    /// <summary>
    /// Player's steam ID
    /// </summary>
    [JsonPropertyName("steamid")]
    public string SteamID { get; set; } = string.Empty;

    /// <summary>
    /// Observer Slot
    /// </summary>
    public int ObserverSlot  { get; set; }

    /// <summary>
    /// Player's name
    /// </summary>
    public string Name  { get; set; }

    /// <summary>
    /// Player's team
    /// </summary>
    [JsonConverter(typeof(JsonStringEnumConverter<PlayerTeam>))]
    public PlayerTeam Team { get; set; }

    /// <summary>
    /// Player's clan tag
    /// </summary>
    public string Clan  { get; set; }

    /// <summary>
    /// Player's current activity state
    /// </summary>
    [JsonConverter(typeof(JsonStringEnumConverter<PlayerActivity>))]
    public PlayerActivity Activity { get; set; }

    /// <summary>
    /// Player's current weapons
    /// </summary>
    [JsonIgnore]
    public WeaponsNode Weapons { get; set; } = WeaponsNode.Default;

    [JsonPropertyName("weapons")]
    public Dictionary<string, WeaponNode> WeaponsDictionary
    {
        get => Weapons.WeaponNodes;
        set => Weapons = new WeaponsNode(value);
    }

    /// <summary>
    /// Player's match statistics
    /// </summary>
    public MatchStatsNode MatchStats  { get; set; } = MatchStatsNode.Default;

    /// <summary>
    /// Player's state information
    /// </summary>
    public PlayerStateNode State  { get; set; } = PlayerStateNode.Default;
}

/// <summary>
/// Enum for various player activities
/// </summary>
public enum PlayerActivity
{
    /// <summary>
    /// Undefined
    /// </summary>
    Undefined,

    /// <summary>
    /// In a menu
    /// </summary>
    Menu,

    /// <summary>
    /// In a game
    /// </summary>
    Playing,

    /// <summary>
    /// In a console/chat
    /// </summary>
    TextInput
}

/// <summary>
/// Enum for each team
/// </summary>
public enum PlayerTeam
{
    /// <summary>
    /// Undefined
    /// </summary>
    Undefined,

    /// <summary>
    /// Terrorist team
    /// </summary>
    T,

    /// <summary>
    /// Counter-Terrorist team
    /// </summary>
    CT
}