using AuroraRgb.Profiles.Generic;
using AuroraRgb.Profiles.Stationeers.GSI.Nodes;
using Newtonsoft.Json;

namespace AuroraRgb.Profiles.Stationeers.GSI;

public partial class GameStateStationeers : GameState
{
    public static readonly GameStateStationeers Default = new();
    public Provider Provider { get; } = Provider.Default;
    [JsonProperty("gamestate")]
    public GameStateNode GameState { get; } = GameStateNode.Default;    
    public PlayerNode Player { get; } = PlayerNode.Default;
    public WorldNode World { get; } = WorldNode.Default;
}