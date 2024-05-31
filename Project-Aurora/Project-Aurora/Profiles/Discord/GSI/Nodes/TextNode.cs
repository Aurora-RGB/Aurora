namespace AuroraRgb.Profiles.Discord.GSI.Nodes;

public enum DiscordTextType
{
    Undefined = -1,
    TextChannel = 0,
    DirectMessage = 1,
    GroupChat = 3
}

public class TextNode : AutoJsonNode<TextNode> {
    public long Id  { get; set; }
    public string Name { get; set; }
    public DiscordTextType Type { get; set; }

    internal TextNode(string json) : base(json) { }
}