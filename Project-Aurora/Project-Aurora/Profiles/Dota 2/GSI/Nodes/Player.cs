using System.Text.Json.Serialization;

namespace AuroraRgb.Profiles.Dota_2.GSI.Nodes;

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
    Playing
}

/// <summary>
/// Class representing player information
/// </summary>
public class PlayerDota2
{
    public static readonly PlayerDota2 Default = new();

    /// <summary>
    /// Player's steam ID
    /// </summary>
    [JsonPropertyName("steamid")]
    public string SteamID { get; set; } = string.Empty;

    /// <summary>
    /// Player's name
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Player's current activity state
    /// </summary>
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public PlayerActivity Activity { get; set; }

    /// <summary>
    /// Player's amount of kills
    /// </summary>
    public int Kills { get; set; }

    /// <summary>
    /// Player's amount of deaths
    /// </summary>
    public int Deaths { get; set; }

    /// <summary>
    /// Player's amount of assists
    /// </summary>
    public int Assists { get; set; }

    /// <summary>
    /// Player's amount of last hits
    /// </summary>
    public int LastHits { get; set; }

    /// <summary>
    /// Player's amount of denies
    /// </summary>
    public int Denies { get; set; }

    /// <summary>
    /// Player's killstreak
    /// </summary>
    public int KillStreak { get; set; }

    /// <summary>
    /// Player's team
    /// </summary>
    [JsonPropertyName("team_name")]
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public PlayerTeam Team { get; set; }

    /// <summary>
    /// Player's amount of gold
    /// </summary>
    public int Gold { get; set; }

    /// <summary>
    /// Player's amount of reliable gold
    /// </summary>
    public int GoldReliable { get; set; }

    /// <summary>
    /// Player's amount of unreliable gold
    /// </summary>
    public int GoldUnreliable { get; set; }

    /// <summary>
    /// PLayer's gold per minute
    /// </summary>
    [JsonPropertyName("gpm")]
    public int GoldPerMinute { get; set; }

    /// <summary>
    /// Player's experience per minute
    /// </summary>
    [JsonPropertyName("xpm")]
    public int ExperiencePerMinute { get; set; }
}