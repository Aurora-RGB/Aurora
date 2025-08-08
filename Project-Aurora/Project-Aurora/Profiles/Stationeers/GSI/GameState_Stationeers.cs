using AuroraRgb.Profiles.Generic;
using AuroraRgb.Profiles.Stationeers.GSI.Nodes;

namespace AuroraRgb.Profiles.Stationeers.GSI;

public partial class GameStateStationeers : GameState
{
    public static readonly GameStateStationeers Default = new();
    public ProviderNode Provider { get; } = ProviderNode.Default;
    public GameStateNode GameState { get; } = GameStateNode.Default;    
    public PlayerNode Player { get;  } = PlayerNode.Default;
    public WorldNode WorldNode { get; } = WorldNode.Default;
}