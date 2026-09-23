using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;

namespace AuroraRgb.Profiles.RocketLeague.GSI.Nodes;

/// <summary>
/// Class representing match information
/// </summary>
[method: JsonConstructor]
public class RlGame(
    IReadOnlyList<RlTeam> teams,
    int timeSeconds,
    bool overtime,
    int frame,
    double elapsed,
    RlBall? ball,
    bool isReplay,
    bool hasWinner,
    string? winner,
    string? arena,
    bool hasTarget,
    RlTarget? target)
{
    public static RlGame Default => new(
        [], 0, false, 0, 0.0, null,
        false, false, null, null, false, null
    );
    
    [JsonPropertyName("Teams")]
    public IReadOnlyList<RlTeam> Teams { get; } = teams;
    
    [JsonIgnore]
    public RlTeam? Blue => Teams.ElementAtOrDefault(0);
    
    [JsonIgnore]
    public RlTeam? Orange => Teams.ElementAtOrDefault(1);
    
    [JsonIgnore]
    public int TotalGoals => Teams.Sum(t => t.Goals);

    [JsonPropertyName("TimeSeconds")]
    public int TimeSeconds { get; } = timeSeconds;

    [JsonPropertyName("bOvertime")]
    public bool Overtime { get; } = overtime;

    [JsonPropertyName("Frame")]
    public int Frame { get; } = frame;

    [JsonPropertyName("Elapsed")]
    public double Elapsed { get; } = elapsed;

    [JsonPropertyName("Ball")]
    public RlBall? Ball { get; } = ball;

    [JsonPropertyName("bReplay")]
    public bool IsReplay { get; } = isReplay;

    [JsonPropertyName("bHasWinner")]
    public bool HasWinner { get; } = hasWinner;

    [JsonPropertyName("Winner")]
    public string? Winner { get; } = winner;

    [JsonPropertyName("Arena")]
    public string? Arena { get; } = arena;

    [JsonPropertyName("bHasTarget")]
    public bool HasTarget { get; } = hasTarget;

    [JsonPropertyName("Target")]
    public RlTarget? Target { get; } = target;
}

[method: JsonConstructor]
public class RlBall(
    double speed,
    int teamNum)
{
    [JsonPropertyName("Speed")]
    public double Speed { get; } = speed;

    [JsonPropertyName("TeamNum")]
    public int TeamNum { get; } = teamNum;
}