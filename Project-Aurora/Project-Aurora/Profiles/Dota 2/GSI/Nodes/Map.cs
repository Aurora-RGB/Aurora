using System.Text.Json.Serialization;

namespace AuroraRgb.Profiles.Dota_2.GSI.Nodes;

/// <summary>
/// Enum list for each Game State
/// </summary>
public enum DOTA_GameState
{
    /// <summary>
    /// Undefined
    /// </summary>
    Undefined = 0,

    /// <summary>
    /// Disconnected
    /// </summary>
    DOTA_GAMERULES_STATE_DISCONNECT = 1,

    /// <summary>
    /// Game is in progress
    /// </summary>
    DOTA_GAMERULES_STATE_GAME_IN_PROGRESS = 2,

    /// <summary>
    /// Players are currently selecting heroes
    /// </summary>
    DOTA_GAMERULES_STATE_HERO_SELECTION = 3,

    /// <summary>
    /// Game is starting
    /// </summary>
    DOTA_GAMERULES_STATE_INIT = 4,

    /// <summary>
    /// Game is ending
    /// </summary>
    DOTA_GAMERULES_STATE_LAST = 5,

    /// <summary>
    /// Game has ended, post game scoreboard
    /// </summary>
    DOTA_GAMERULES_STATE_POST_GAME = 6,

    /// <summary>
    /// Game has started, pre game preparations
    /// </summary>
    DOTA_GAMERULES_STATE_PRE_GAME = 7,

    /// <summary>
    /// Players are selecting/banning heroes
    /// </summary>
    DOTA_GAMERULES_STATE_STRATEGY_TIME = 8,

    /// <summary>
    /// Waiting for everyone to connect and load
    /// </summary>
    DOTA_GAMERULES_STATE_WAIT_FOR_PLAYERS_TO_LOAD = 9,

    /// <summary>
    /// Game is a custom game
    /// </summary>
    DOTA_GAMERULES_STATE_CUSTOM_GAME_SETUP = 10
}

/// <summary>
/// Enum list for each player team
/// </summary>
public enum DotaPlayerTeam
{
    /// <summary>
    /// Undefined
    /// </summary>
    Undefined,

    /// <summary>
    /// No team
    /// </summary>
    None,

    /// <summary>
    /// Dire team
    /// </summary>
    Dire,

    /// <summary>
    /// Radiant team
    /// </summary>
    Radiant
}
    
/// <summary>
/// Class representing information about the map
/// </summary>
public class Map_Dota2
{
    public static readonly Map_Dota2 Default = new();

    /// <summary>
    /// Name of the current map
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Match ID of the current game
    /// </summary>
    [AutoJsonPropertyName("matchid")]
    public long MatchID { get; set; }

    /// <summary>
    /// Game time
    /// </summary>
    public int GameTime { get; set; }

    /// <summary>
    /// Clock time (time shown at the top of the game hud)
    /// </summary>
    public int ClockTime { get; set; }

    /// <summary>
    /// A boolean representing whether it is daytime
    /// </summary>
    [JsonPropertyName("daytime")]
    public bool IsDaytime { get; set; }

    /// <summary>
    /// A boolean representing whether Nightstalker forced night time
    /// </summary>
    [JsonPropertyName("nightstalker_night")]
    public bool IsNightstalker_Night { get; set; }

    /// <summary>
    /// Current game state
    /// </summary>
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public DOTA_GameState GameState { get; set; }

    /// <summary>
    /// The winning team
    /// </summary>
    [JsonPropertyName("win_team")]
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public DotaPlayerTeam Win_team { get; set; }

    /// <summary>
    /// The name of the custom game
    /// </summary>
    [JsonPropertyName("customgamename")]
    public string CustomGameName { get; set; } = string.Empty;

    /// <summary>
    /// The cooldown on ward purchases
    /// </summary>
    [JsonPropertyName("ward_purchase_cooldown")]
    public int Ward_Purchase_Cooldown { get; set; }
}