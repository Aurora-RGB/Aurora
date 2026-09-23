using System.ComponentModel;
using System.Linq;
using System.Text.Json.Serialization;
using AuroraRgb.Profiles.RocketLeague.GSI.Nodes;

namespace AuroraRgb.Profiles.RocketLeague.GSI;

public enum RlStatus
{
    [Description("Menu")]
    Undefined = -1,
    [Description("In Game")]
    InGame,
}

public partial class GameStateRocketLeague : GameState
{
    public RlData Data { get; set; } = RlData.Default;

    public RlPlayer? Player => Data.Players
        .FirstOrDefault(p => p.Shortcut == Data.Game.Target?.Shortcut);

    public RlGame Game => Data.Game;

    public RlStatus GameStatus { get; set; } = RlStatus.Undefined;

    public RlTeam? OpponentTeam => Data.Game.Teams
        .FirstOrDefault(t => t.TeamNum != Data.Game.Target?.TeamNum);

    public RlTeam? YourTeam => Data.Game.Teams
        .FirstOrDefault(t => t.TeamNum == Data.Game.Target?.TeamNum);

    public RlTeam? HighlightedTeam
    {
        get => YourTeam ?? field;
        set;
    }
}

[method: JsonConstructor]
public class RlData(
    string matchGuid,
    RlPlayer[] players,
    RlGame? game)
{
    public static RlData Default => new(string.Empty, [], null);

    [JsonPropertyName("MatchGuid")]
    public string MatchGuid { get; } = matchGuid;

    [JsonPropertyName("Players")]
    public RlPlayer[] Players { get; } = players;

    [JsonPropertyName("Game")]
    public RlGame Game { get; } = game ?? RlGame.Default;
}
