using System.Text.Json.Serialization;

namespace AuroraRgb.Profiles.Discord.GSI.Nodes;

public class VoiceParticipantNode
{
    [JsonPropertyName("userId")]
    public string UserId { get; set; } = string.Empty;
    
    [JsonPropertyName("is_speaking")]
    public bool IsSpeaking { get; set; }
    
    [JsonPropertyName("mute")]
    public bool IsMuted { get; set; }
    
    [JsonPropertyName("deaf")]
    public bool IsDeafened { get; set; }
    
    [JsonPropertyName("selfMute")]
    public bool IsSelfMuted { get; set; }
    
    [JsonPropertyName("selfDeaf")]
    public bool IsSelfDeafened { get; set; }
}