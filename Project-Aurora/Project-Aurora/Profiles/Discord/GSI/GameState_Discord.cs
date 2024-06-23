using System.Text.Json.Serialization;
using AuroraRgb.Profiles.Discord.GSI.Nodes;
using AuroraRgb.Profiles.Generic;

namespace AuroraRgb.Profiles.Discord.GSI;

public partial class GameStateDiscord : GameState
{
    public static readonly GameStateDiscord Default = new();
    
    [JsonPropertyName("provider")]
    public ProviderNode Provider { get; set; } = ProviderNode.Default;

    [JsonPropertyName("user")]
    public UserNode User { get; set; } = UserNode.Default;

    [JsonPropertyName("guild")]
    public GuildNode Guild { get; set; } = GuildNode.Default;

    [JsonPropertyName("text")]
    public TextNode Text { get; set; } = TextNode.Default;

    [JsonPropertyName("voice")]
    public VoiceNode Voice { get; set; } = VoiceNode.Default;
}