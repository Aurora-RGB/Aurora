using System.Text.Json.Serialization;

namespace AuroraRgb.Profiles.Discord.GSI.Nodes;

public class GuildNode {
    public static readonly GuildNode Default = new();

    [JsonPropertyName("id")]
    public long Id { get; set; }
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;
}