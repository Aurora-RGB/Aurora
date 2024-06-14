namespace AuroraRgb.Profiles.RocketLeague.GSI.Nodes;

/// <summary>
/// Class representing player information
/// </summary>
public class Player_RocketLeague : AutoJsonNode<Player_RocketLeague>
{
    /// <summary>
    /// The Index of the team the player is on
    /// </summary>
    public int Team { get; set; } = -1;

    /// <summary>
    /// Amount of boost (0-1)
    /// </summary>
    public float Boost { get; set; } = -1;

    /// <summary>
    /// Number of points the player has on the scoreboard
    /// </summary>
    public int Score { get; set; } = -1;

    /// <summary>
    /// Number of goals the player scored
    /// </summary>
    public int Goals { get; set; } = -1;

    /// <summary>
    /// Number of assists the player has
    /// </summary>
    public int Assists { get; set; } = -1;

    /// <summary>
    /// Number of saves the player has
    /// </summary>
    public int Saves { get; set; } = -1;

    /// <summary>
    /// Number of shots the player has
    /// </summary>
    public int Shots { get; set; } = -1;

    internal Player_RocketLeague(string jsonData) : base(jsonData) { }
}