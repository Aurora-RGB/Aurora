using System.Text.Json.Serialization;

namespace AuroraRgb.Profiles.CSGO.GSI.Nodes;

/// <summary>
/// Class representing information about the map
/// </summary>
public class MapNode
{
    public static readonly MapNode Default = new();

    /// <summary>
    /// Current gamemode
    /// </summary>
    [JsonConverter(typeof(JsonStringEnumConverter<MapMode>))]
    public MapMode Mode { get; set; } = MapMode.Undefined;

    /// <summary>
    /// Name of the current map
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Current phase of the map
    /// </summary>
    [JsonConverter(typeof(JsonStringEnumConverter<MapPhase>))]
    public MapPhase Phase { get; set; } = MapPhase.Undefined;

    /// <summary>
    /// Current round
    /// </summary>
    public int Round { get; set; }

    /// <summary>
    /// Counter-Terrorist team information
    /// </summary>
    [JsonPropertyName("team_ct")]
    public MapTeamNode TeamCT { get; set; } = MapTeamNode.Default;

    /// <summary>
    /// Terrorist team information
    /// </summary>
    [JsonPropertyName("team_t")]
    public MapTeamNode TeamT { get; set; } = MapTeamNode.Default;
}

/// <summary>
/// Enum list for each phase of the map
/// </summary>
public enum MapPhase
{
    /// <summary>
    /// Undefined phase
    /// </summary>
    Undefined,

    /// <summary>
    /// Warmup phase
    /// </summary>
    Warmup,

    /// <summary>
    /// Live match phase
    /// </summary>
    Live,

    /// <summary>
    /// Intermission phase
    /// </summary>
    Intermission,

    /// <summary>
    /// Match Over phase
    /// </summary>
    GameOver
}

public enum MapMode
{
    /// <summary>
    /// Undefined gamemode
    /// </summary>
    Undefined,

    /// <summary>
    /// Casual gamemode
    /// </summary>
    Casual,

    /// <summary>
    /// Competitive gamemode
    /// </summary>
    Competitive,

    /// <summary>
    /// Deathmatch gamemode
    /// </summary>
    DeathMatch,
    /// <summary>
    /// Gun Game
    /// </summary>
    GunGameProgressive,

    /// <summary>
    /// Arms Race/Demolition gamemode
    /// </summary>
    GunGameTRBomb,

    /// <summary>
    /// Cooperational mission gamemode
    /// </summary>
    CoopMission,

    /// <summary>
    /// Custom gamemode
    /// </summary>
    Custom
}