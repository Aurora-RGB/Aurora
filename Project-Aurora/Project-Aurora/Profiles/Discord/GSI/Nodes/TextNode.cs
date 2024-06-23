using System.Text.Json.Serialization;

namespace AuroraRgb.Profiles.Discord.GSI.Nodes;

public enum DiscordTextType
{
    Undefined = -1,
    TextChannel = 0,
    DirectMessage = 1,
    GroupChat = 3
}

public class TextNode
{
    public static readonly TextNode Default = new();

    [JsonPropertyName("id")]
    public long Id { get; set; }
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;
    [JsonPropertyName("type")]
    public DiscordTextType Type { get; set; } = DiscordTextType.Undefined;
}