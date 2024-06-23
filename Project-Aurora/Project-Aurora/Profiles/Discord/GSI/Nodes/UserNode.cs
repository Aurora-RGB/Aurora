using System.Text.Json.Serialization;

namespace AuroraRgb.Profiles.Discord.GSI.Nodes;

public enum DiscordStatus
{
    Undefined,
    Online,
    Idle,
    DoNotDisturb,
    Invisible
}

public class UserNode
{
    public static readonly UserNode Default = new();

    [JsonPropertyName("id")]
    public long Id { get; set; }

    [JsonIgnore]
    public DiscordStatus Status { get; private set; }

    [JsonPropertyName("status")]
    public string StatusJson
    {
        set => Status = GetStatus(value);
    }

    [JsonPropertyName("self_mute")]
    public bool SelfMute { get; set; }

    [JsonPropertyName("self_deafen")]
    public bool SelfDeafen { get; set; }

    public bool Mentions { get; set; }

    [JsonPropertyName("unread_messages")]
    public bool UnreadMessages { get; set; }

    [JsonPropertyName("being_called")]
    public bool BeingCalled { get; set; }

    [JsonPropertyName("is_speaking")]
    public bool IsSpeaking { get; set; }

    private static DiscordStatus GetStatus(string status)
    {
        switch (status)
        {
            case "online":
                return DiscordStatus.Online;
            case "dnd":
                return DiscordStatus.DoNotDisturb;
            case "invisible":
                return DiscordStatus.Invisible;
            case "idle":
                return DiscordStatus.Idle;
            default:
                return DiscordStatus.Undefined;
        }
    }
}