using System.Text.Json.Serialization;

namespace AuroraRgb.Profiles.Discord.GSI.Nodes;

public enum DiscordVoiceType
{
    Undefined = -1,
    Call = 1,
    VoiceChannel = 2,
}

public class VoiceNode {
    
    public static readonly VoiceNode Default = new();

    [JsonPropertyName("id")]
    public long Id  { get; set; }
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;
    [JsonPropertyName("type")]
    public DiscordVoiceType Type { get; set; } = DiscordVoiceType.Undefined;
    [JsonPropertyName("somebody_speaking")]
    public bool SomebodySpeaking { get; set; }
}