using AuroraRgb.Nodes;

namespace AuroraRgb.Profiles.CSGO.GSI.Nodes;

/// <summary>
/// Class representing information about the map
/// </summary>
public class MapNode : Node
{
    /// <summary>
    /// Current gamemode
    /// </summary>
    public MapMode Mode { get; }

    /// <summary>
    /// Name of the current map
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// Current phase of the map
    /// </summary>
    public MapPhase Phase { get; }

    /// <summary>
    /// Current round
    /// </summary>
    public int Round { get; }

    /// <summary>
    /// Counter-Terrorist team information
    /// </summary>
    public MapTeamNode TeamCT { get; }

    /// <summary>
    /// Terrorist team information
    /// </summary>
    public MapTeamNode TeamT { get; }

    internal MapNode(string json)
        : base(json)
    {
        Mode = GetEnum<MapMode>("mode");
        Name = GetString("name");
        Phase = GetEnum<MapPhase>("phase");
        Round = GetInt("round");
        TeamCT = new MapTeamNode(_ParsedData["team_ct"]?.ToString() ?? "");
        TeamT = new MapTeamNode(_ParsedData["team_t"]?.ToString() ?? "");
    }
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