using AuroraRgb.Nodes;

namespace AuroraRgb.Profiles.CSGO.GSI.Nodes;

/// <summary>
/// Class representing team information
/// </summary>
public class MapTeamNode : Node
{
    /// <summary>
    /// Team score
    /// </summary>
    public int Score { get; }

    /// <summary>
    /// Remaining Timeouts
    /// </summary>
    public int TimeoutsRemaining { get; }

    internal MapTeamNode(string json)
        : base(json)
    {
        Score = GetInt("score");
        TimeoutsRemaining = GetInt("timeouts_remaining");
    }
}