namespace AuroraRgb.Profiles.CSGO.GSI.Nodes;

/// <summary>
/// Class representing team information
/// </summary>
public class MapTeamNode
{
    public static readonly MapTeamNode Default = new();

    /// <summary>
    /// Team score
    /// </summary>
    public int Score { get; set; }

    /// <summary>
    /// Remaining Timeouts
    /// </summary>
    public int TimeoutsRemaining { get; set; }
}