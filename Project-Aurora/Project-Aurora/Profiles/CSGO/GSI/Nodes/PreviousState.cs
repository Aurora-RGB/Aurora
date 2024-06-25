using System.Text.Json.Serialization;
using AuroraRgb.Profiles.CSGO.GSI.Nodes.Converters;

namespace AuroraRgb.Profiles.CSGO.GSI.Nodes;

public class PreviousState
{
    /// <summary>
    /// Information about the current round
    /// </summary>
    [JsonPropertyName("round")]
    [JsonConverter(typeof(PreviousNodeConverter<RoundNode>))]
    public RoundNode Round { get; set; } = RoundNode.Default;

    [JsonPropertyName("player")]
    [JsonConverter(typeof(PreviousNodeConverter<PlayerNode>))]
    public PlayerNode Player { get; set; } = PlayerNode.Default;
}