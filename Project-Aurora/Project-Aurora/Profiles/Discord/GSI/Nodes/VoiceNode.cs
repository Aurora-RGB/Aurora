namespace AuroraRgb.Profiles.Discord.GSI.Nodes;

public enum DiscordVoiceType
{
    Undefined = -1,
    Call = 1,
    VoiceChannel = 2,
}

public class VoiceNode : AutoJsonNode<VoiceNode> {
    public long Id  { get; set; }
    public string Name { get; set; } = string.Empty;
    public DiscordVoiceType Type { get; set; }

    internal VoiceNode(string json) : base(json) { }
}