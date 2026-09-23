using System.Text.Json.Serialization;
using AuroraRgb.Profiles.RocketLeague.GSI.Nodes;

namespace AuroraRgb.Profiles.RocketLeague.GSI.Events;

[method: JsonConstructor]
public class RlGoalScored(
    string matchGuid,
    double goalSpeed,
    double goalTime,
    RlImpactLocation impactLocation,
    RlTarget scorer,
    RlAssister? assister,
    RlBallLastTouch ballLastTouch)
{
    [JsonPropertyName("MatchGuid")]
    public string MatchGuid { get; } = matchGuid;

    [JsonPropertyName("GoalSpeed")]
    public double GoalSpeed { get; } = goalSpeed;

    [JsonPropertyName("GoalTime")]
    public double GoalTime { get; } = goalTime;

    [JsonPropertyName("ImpactLocation")]
    public RlImpactLocation ImpactLocation { get; } = impactLocation;

    [JsonPropertyName("Scorer")]
    public RlTarget Scorer { get; } = scorer;

    [JsonPropertyName("Assister")]
    public RlAssister? Assister { get; } = assister;

    [JsonPropertyName("BallLastTouch")]
    public RlBallLastTouch BallLastTouch { get; } = ballLastTouch;
}

[method: JsonConstructor]
public class RlAssister(
    string name,
    int shortcut,
    int teamNum)
{
    [JsonPropertyName("Name")]
    public string Name { get; } = name;

    [JsonPropertyName("Shortcut")]
    public int Shortcut { get; } = shortcut;

    [JsonPropertyName("TeamNum")]
    public int TeamNum { get; } = teamNum;
}

[method: JsonConstructor]
public class RlBallLastTouch(
    RlTarget player,
    double speed)
{
    [JsonPropertyName("Player")]
    public RlTarget Player { get; } = player;

    [JsonPropertyName("Speed")]
    public double Speed { get; } = speed;
}

[method: JsonConstructor]
public class RlImpactLocation(
    double x,
    double y,
    double z)
{
    [JsonPropertyName("X")]
    public double X { get; } = x;

    [JsonPropertyName("Y")]
    public double Y { get; } = y;

    [JsonPropertyName("Z")]
    public double Z { get; } = z;
}