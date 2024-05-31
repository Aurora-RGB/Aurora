using AuroraRgb.Nodes;

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
public class PlayerDota2 : Node
{
    /// <summary>
    /// Player's steam ID
    /// </summary>
    public string SteamID { get; }

    /// <summary>
    /// Player's name
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// Player's current activity state
    /// </summary>
    public PlayerActivity Activity { get; }

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
    public int Assists { get; }

    /// <summary>
    /// Player's amount of last hits
    /// </summary>
    public int LastHits { get; }

    /// <summary>
    /// Player's amount of denies
    /// </summary>
    public int Denies { get; }

    /// <summary>
    /// Player's killstreak
    /// </summary>
    public int KillStreak { get; set; }

    /// <summary>
    /// Player's team
    /// </summary>
    public PlayerTeam Team { get; set; }

    /// <summary>
    /// Player's amount of gold
    /// </summary>
    public int Gold { get; }

    /// <summary>
    /// Player's amount of reliable gold
    /// </summary>
    public int GoldReliable { get; }

    /// <summary>
    /// Player's amount of unreliable gold
    /// </summary>
    public int GoldUnreliable { get; }

    /// <summary>
    /// PLayer's gold per minute
    /// </summary>
    public int GoldPerMinute { get; }

    /// <summary>
    /// Player's experience per minute
    /// </summary>
    public int ExperiencePerMinute { get; }

    internal PlayerDota2(string jsonData) : base(jsonData)
    {
        SteamID = GetString("steamid");
        Name = GetString("name");
        Activity = GetEnum<PlayerActivity>("activity");
        Kills = GetInt("kills");
        Deaths = GetInt("deaths");
        Assists = GetInt("assists");
        LastHits = GetInt("last_hits");
        Denies = GetInt("denies");
        KillStreak = GetInt("kill_streak");
        Team = GetEnum<PlayerTeam>("team_name");
        Gold = GetInt("gold");
        GoldReliable = GetInt("gold_reliable");
        GoldUnreliable = GetInt("gold_unreliable");
        GoldPerMinute = GetInt("gpm");
        ExperiencePerMinute = GetInt("xpm");
    }
}