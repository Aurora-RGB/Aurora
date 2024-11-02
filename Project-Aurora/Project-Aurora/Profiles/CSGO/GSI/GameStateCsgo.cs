using System.Text.Json.Serialization;
using AuroraRgb.Profiles.CSGO.GSI.Nodes;
using AuroraRgb.Profiles.CSGO.GSI.Nodes.Converters;
using AuroraRgb.Profiles.Dota_2.GSI.Nodes;

namespace AuroraRgb.Profiles.CSGO.GSI;

/// <summary>
/// A class representing various information retaining to Game State Integration of Counter-Strike: Global Offensive
/// </summary>
public partial class GameStateCsgo : GameState
{
    /// <summary>
    /// Information about the provider of this GameState
    /// </summary>
    [JsonPropertyName("provider")]
    public ProviderValve Provider { get; set; } = ProviderValve.Default;

    /// <summary>
    /// Information about the current map
    /// </summary>
    [JsonPropertyName("map")]
    public MapNode Map { get; set; } = MapNode.Default;

    /// <summary>
    /// Information about the current round
    /// </summary>
    [JsonPropertyName("round")]
    public RoundNode Round { get; set; } = RoundNode.Default;

    /// <summary>
    /// Information about the current player
    /// </summary>
    [JsonPropertyName("player")]
    public PlayerNode Player { get; set; } = PlayerNode.Default;

    /// <summary>
    /// A previous GameState
    /// </summary>
    [JsonPropertyName("previously")]
    [JsonConverter(typeof(PreviousNodeConverter<PreviousState>))]
    public PreviousState Previously { get; set; } = PreviousState.Default;

    /// <summary>
    /// Information about GSI authentication
    /// </summary>
    [JsonPropertyName("auth")]
    public AuthNode Auth { get; set; } = AuthNode.Default;
}