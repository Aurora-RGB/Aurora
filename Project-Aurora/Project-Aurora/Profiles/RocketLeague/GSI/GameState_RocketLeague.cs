using System.ComponentModel;
using AuroraRgb.Profiles.RocketLeague.GSI.Nodes;

namespace AuroraRgb.Profiles.RocketLeague.GSI;

public enum RLStatus
{
    [Description("Menu")]
    Undefined = -1,
    [Description("Replay")]
    Replay,
    [Description("In Game")]
    InGame,
    [Description("Freeplay")]
    Freeplay,
    [Description("Training")]
    Training,
    [Description("Spectate")]
    Spectate,
    [Description("Local Match")]
    Local
};

public class GameRocketLeague : AutoJsonNode<GameRocketLeague>
{
    public RLStatus Status { get; set; }

    internal GameRocketLeague(string jsonData) : base(jsonData) { }
}

/// <summary>
/// A class representing various information relating to Rocket League
/// </summary>
public partial class GameStateRocketLeague : NewtonsoftGameState
{
    public Match_RocketLeague Match => NodeFor<Match_RocketLeague>("match");
    public Player_RocketLeague Player => NodeFor<Player_RocketLeague>("player");
    public GameRocketLeague Game => NodeFor<GameRocketLeague>("game");

    /// <summary>
    /// Creates a default GameState_RocketLeague instance.
    /// </summary>
    public GameStateRocketLeague() { }

    /// <summary>
    /// Creates a GameState instance based on the passed json data.
    /// </summary>
    /// <param name="jsonData">The passed json data</param>
    public GameStateRocketLeague(string jsonData) : base(jsonData) { }

    /// <summary>
    /// Returns true if all the color values for both teams are between zero and one.
    /// </summary>
    /// <returns></returns>
    public bool ColorsValid()
    {
        return Match.Orange.Red >= 0 && Match.Blue.Red <= 1 &&
               Match.Orange.Green >= 0 && Match.Blue.Green <= 1 &&
               Match.Orange.Blue >= 0 && Match.Blue.Blue <= 1 &&
               Match.Orange.Red >= 0 && Match.Blue.Red <= 1 &&
               Match.Orange.Green >= 0 && Match.Blue.Green <= 1 &&
               Match.Orange.Blue >= 0 && Match.Blue.Blue <= 1;
    }

    public Team_RocketLeague OpponentTeam => Player.Team == 0 ? Match.Orange : Match.Blue;

    public Team_RocketLeague YourTeam => Player.Team == 0 ? Match.Blue : Match.Orange;
}