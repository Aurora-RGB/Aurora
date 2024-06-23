using System.Text.Json.Serialization;

namespace AuroraRgb.Profiles.CSGO.GSI.Nodes;

/// <summary>
/// Class representing various player statistics
/// </summary>
public class MatchStatsNode
{
    public static readonly MatchStatsNode Default = new();

    /// <summary>
    /// Amount of kills
    /// </summary>
    public int Kills { get; set; }

    /// <summary>
    /// Amount of assists
    /// </summary>
    public int Assists { get; set; }

    /// <summary>
    /// Amount of deaths
    /// </summary>
    public int Deaths { get; set; }

    /// <summary>
    /// Amount of MVPs
    /// </summary>
    [JsonPropertyName("mvps")]
    public int MVPs { get; set; }

    /// <summary>
    /// The score
    /// </summary>
    public int Score { get; set; }
}