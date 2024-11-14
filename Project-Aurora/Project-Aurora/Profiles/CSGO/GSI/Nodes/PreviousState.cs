using System.Text.Json.Serialization;
using AuroraRgb.Profiles.CSGO.GSI.Nodes.Converters;
using Common.Utils;

namespace AuroraRgb.Profiles.CSGO.GSI.Nodes;

public class PreviousState
{
    public static readonly PreviousState Default = new()
    {
        Round = (RoundNode)new RoundNode().TryClone(true),
        Player = (PlayerNode)new PlayerNode().TryClone(true),
    };

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