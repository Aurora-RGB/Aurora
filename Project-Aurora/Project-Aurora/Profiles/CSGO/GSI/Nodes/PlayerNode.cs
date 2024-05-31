using AuroraRgb.Nodes;

namespace AuroraRgb.Profiles.CSGO.GSI.Nodes;

/// <summary>
/// Class representing player information
/// </summary>
public class PlayerNode : Node
{
    internal string _SteamID { get; set; }

    /// <summary>
    /// Player's steam ID
    /// </summary>
    public string SteamID => _SteamID;

    /// <summary>
    /// Observer Slot
    /// </summary>
    public int ObserverSlot { get; }

    /// <summary>
    /// Player's name
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// Player's team
    /// </summary>
    public PlayerTeam Team { get; set; }

    /// <summary>
    /// Player's clan tag
    /// </summary>
    public string Clan { get; }

    /// <summary>
    /// Player's current activity state
    /// </summary>
    public PlayerActivity Activity { get; set; }

    /// <summary>
    /// Player's current weapons
    /// </summary>
    public WeaponsNode Weapons { get; }

    /// <summary>
    /// Player's match statistics
    /// </summary>
    public MatchStatsNode MatchStats { get; }

    /// <summary>
    /// Player's state information
    /// </summary>
    public PlayerStateNode State { get; }

    internal PlayerNode(string json)
        : base(json)
    {
        _SteamID = GetString("steamid");
        ObserverSlot = GetInt("observer_slot");
        Name = GetString("name");
        Team = GetEnum<PlayerTeam>("team");
        Clan = GetString("clan");
        State = new PlayerStateNode(_ParsedData?.SelectToken("state")?.ToString() ?? "{}");
        Weapons = new WeaponsNode(_ParsedData?.SelectToken("weapons")?.ToString() ?? "{}");
        MatchStats = new MatchStatsNode(_ParsedData?.SelectToken("match_stats")?.ToString() ?? "{}");
        Activity = GetEnum<PlayerActivity>("activity");
    }
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