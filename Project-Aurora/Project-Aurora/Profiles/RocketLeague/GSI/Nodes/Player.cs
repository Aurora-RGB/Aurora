using System.Text.Json.Serialization;

namespace AuroraRgb.Profiles.RocketLeague.GSI.Nodes;

/// <summary>
/// Class representing player information
/// </summary>
[method: JsonConstructor]
public class RlPlayer(
    string name,
    string primaryId,
    int shortcut,
    int teamNum,
    int score,
    int goals,
    int shots,
    int assists,
    int saves,
    int touches,
    int carTouches,
    int demos,
    bool hasCar,
    double speed,
    int boost,
    bool boosting,
    bool onGround,
    bool onWall,
    bool powersliding,
    bool demolished,
    RlAttacker? attacker,
    bool supersonic)
{
    [JsonPropertyName("Name")]
    public string Name { get; } = name;

    [JsonPropertyName("PrimaryId")]
    public string PrimaryId { get; } = primaryId;

    [JsonPropertyName("Shortcut")]
    public int Shortcut { get; } = shortcut;

    [JsonPropertyName("TeamNum")]
    public int TeamNum { get; set; } = teamNum;

    [JsonPropertyName("Score")]
    public int Score { get; } = score;

    [JsonPropertyName("Goals")]
    public int Goals { get; } = goals;

    [JsonPropertyName("Shots")]
    public int Shots { get; } = shots;

    [JsonPropertyName("Assists")]
    public int Assists { get; } = assists;

    [JsonPropertyName("Saves")]
    public int Saves { get; } = saves;

    [JsonPropertyName("Touches")]
    public int Touches { get; } = touches;

    [JsonPropertyName("CarTouches")]
    public int CarTouches { get; } = carTouches;

    [JsonPropertyName("Demos")]
    public int Demos { get; } = demos;

    [JsonPropertyName("bHasCar")]
    public bool HasCar { get; } = hasCar;

    [JsonPropertyName("Speed")]
    public double Speed { get; } = speed;

    [JsonPropertyName("Boost")]
    public int Boost { get; set; } = boost;

    [JsonPropertyName("bBoosting")]
    public bool Boosting { get; } = boosting;

    [JsonPropertyName("bOnGround")]
    public bool OnGround { get; } = onGround;

    [JsonPropertyName("bOnWall")]
    public bool OnWall { get; } = onWall;

    [JsonPropertyName("bPowersliding")]
    public bool Powersliding { get; } = powersliding;

    [JsonPropertyName("bDemolished")]
    public bool Demolished { get; } = demolished;

    [JsonPropertyName("Attacker")]
    public RlAttacker? Attacker { get; } = attacker;

    [JsonPropertyName("bSupersonic")]
    public bool Supersonic { get; } = supersonic;
}