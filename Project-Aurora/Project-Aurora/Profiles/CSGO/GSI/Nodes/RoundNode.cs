using System.Text.Json.Serialization;

namespace AuroraRgb.Profiles.CSGO.GSI.Nodes;

/// <summary>
/// Class representing information about the round
/// </summary>
public class RoundNode
{
    public static readonly RoundNode Default = new();

    /// <summary>
    /// Current round phase
    /// </summary>
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public RoundPhase Phase { get; set; }

    /// <summary>
    /// Current bomb state
    /// </summary>
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public BombState Bomb { get; set; }

    /// <summary>
    /// Round winning team
    /// </summary>
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public RoundWinTeam WinTeam { get; set; }
}

/// <summary>
/// Enum list for each round phase
/// </summary>
public enum RoundPhase
{
    /// <summary>
    /// Undefined phase
    /// </summary>
    Undefined,

    /// <summary>
    /// Live round phase
    /// </summary>
    Live,

    /// <summary>
    /// Round over phase
    /// </summary>
    Over,

    /// <summary>
    /// Round paused phase
    /// </summary>
    FreezeTime
}

/// <summary>
/// Enum list for each bomb state
/// </summary>
public enum BombState
{
    /// <summary>
    /// Undefined state
    /// </summary>
    Undefined,

    /// <summary>
    /// Bomb plated state
    /// </summary>
    Planted,

    /// <summary>
    /// Bomb exploded state
    /// </summary>
    Exploded,

    /// <summary>
    /// Bomb defused state
    /// </summary>
    Defused
}

/// <summary>
/// Enum lust for each winning team
/// </summary>
public enum RoundWinTeam
{
    /// <summary>
    /// Undefined team
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