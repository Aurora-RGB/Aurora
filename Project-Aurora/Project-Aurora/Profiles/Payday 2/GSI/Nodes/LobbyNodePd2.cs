using System.Text.Json.Serialization;

namespace AuroraRgb.Profiles.Payday_2.GSI.Nodes;

/// <summary>
/// Information about the game lobby
/// </summary>
public class LobbyNodePd2
{
    public static readonly LobbyNodePd2 Default = new();

    /// <summary>
    /// Lobby difficulty
    /// </summary>
    [JsonConverter(typeof(JsonStringEnumConverter<LobbyDifficulty>))]
    public LobbyDifficulty Difficulty { get; set; }

    /// <summary>
    /// Lobby visibility permissions
    /// </summary>
    [JsonPropertyName("permission")]
    [JsonConverter(typeof(JsonStringEnumConverter<LobbyPermissions>))]
    public LobbyPermissions Permissions { get; set; }

    /// <summary>
    /// A boolean representing if team AI is enabled
    /// </summary>
    [JsonPropertyName("team_ai")]
    public bool IsTeamAIEnabled { get; set; }

    /// <summary>
    /// Required level to join this lobby
    /// </summary>
    [JsonPropertyName("minimum_level")]
    public int RequiredLevel { get; set; }

    /// <summary>
    /// A boolean representing if dropping in is enabled
    /// </summary>
    [JsonPropertyName("drop_in")]
    public bool DropInEnabled { get; set; }

    /// <summary>
    /// Lobby kick option
    /// </summary>
    [JsonPropertyName("kick_option")]
    [JsonConverter(typeof(JsonStringEnumConverter<LobbyKickSetting>))]
    public LobbyKickSetting KickSetting { get; set; }

    /// <summary>
    /// Lobby job plan
    /// </summary>
    [JsonPropertyName("job_plan")]
    [JsonConverter(typeof(JsonStringEnumConverter<LobbyJobPlan>))]
    public LobbyJobPlan JobPlan { get; set; }

    /// <summary>
    /// A boolean representing if cheaters are automatically kicked
    /// </summary>
    [JsonPropertyName("cheater_auto_kick")]
    public bool CheaterAutoKick { get; set; }

    /// <summary>
    /// A boolean representing if lobby is singleplayer
    /// </summary>
    [JsonPropertyName("singleplayer")]
    public bool IsSingleplayer { get; set; }
}

/// <summary>
/// Enum for every difficulty level
/// </summary>
public enum LobbyDifficulty
{
    /// <summary>
    /// Undefined
    /// </summary>
    Undefined,

    /// <summary>
    /// Normal difficluty
    /// </summary>
    Normal,

    /// <summary>
    /// Hard difficulty
    /// </summary>
    Hard,

    /// <summary>
    /// Very Hard difficulty
    /// </summary>
    Overkill,

    /// <summary>
    /// Overkill difficulty
    /// </summary>
    Overkill_145,

    /// <summary>
    /// Deathwish difficulty
    /// </summary>
    Overkill_290
}

/// <summary>
/// Enum for each lobby permission
/// </summary>
public enum LobbyPermissions
{
    /// <summary>
    /// Undefined
    /// </summary>
    Undefined,

    /// <summary>
    /// Public lobby
    /// </summary>
    Public,

    /// <summary>
    /// Friends only lobby
    /// </summary>
    Friends_only,

    /// <summary>
    /// Private lobby
    /// </summary>
    Private
}

/// <summary>
/// Enum for lobby kick settings
/// </summary>
public enum LobbyKickSetting
{
    /// <summary>
    /// Undefined
    /// </summary>
    Undefined = -1,

    /// <summary>
    /// No kick
    /// </summary>
    NoKick = 0,

    /// <summary>
    /// Host can kick
    /// </summary>
    HostKick = 1,

    /// <summary>
    /// Vote kicking
    /// </summary>
    VoteKick = 2
}

/// <summary>
/// Enum for lobby job plan
/// </summary>
public enum LobbyJobPlan
{
    /// <summary>
    /// Undefined
    /// </summary>
    Undefined = -1,

    /// <summary>
    /// Loud
    /// </summary>
    Loud = 1,

    /// <summary>
    /// Stealth
    /// </summary>
    Stealth = 2
}