using AuroraRgb.Profiles.Stationeers.GSI.Nodes;

namespace AuroraRgb.Profiles.Stationeers.GSI;

public partial class GameStateStationeers : GameState
{
    public static readonly GameStateStationeers Default = new();
    public Provider Provider { get; set; } = Provider.Default;
    public PlayerNode Player { get; set; } = PlayerNode.Default;
    public WorldNode WorldNode { get; set; } = WorldNode.Default;
}