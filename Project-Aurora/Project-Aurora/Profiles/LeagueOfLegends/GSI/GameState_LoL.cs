using AuroraRgb.Profiles.LeagueOfLegends.GSI.Nodes;

namespace AuroraRgb.Profiles.LeagueOfLegends.GSI;

public class GameState_LoL : NewtonsoftGameState
{
    private PlayerNode? player;
    public PlayerNode Player => player ??= new PlayerNode();

    private MatchNode? match;
    public MatchNode Match => match ??= new MatchNode();

    public GameState_LoL()
    {

    }

    public GameState_LoL(string jsonData) : base(jsonData)
    {

    }
}