﻿namespace AuroraRgb.Profiles.Discord.GSI.Nodes;

public enum DiscordStatus
{
    Undefined,
    Online,
    Idle,
    DoNotDisturb,
    Invisible
}

public class UserNode : AutoJsonNode<UserNode> {
    public long Id = 0;
    [AutoJsonIgnore] public DiscordStatus Status { get; set; }
    [AutoJsonPropertyName("self_mute")] public bool SelfMute { get; set; }
    [AutoJsonPropertyName("self_deafen")] public bool SelfDeafen { get; set; }
    public bool Mentions { get; set; }
    [AutoJsonPropertyName("unread_messages")] public bool UnreadMessages { get; set; }
    [AutoJsonPropertyName("being_called")] public bool BeingCalled { get; set; }

    internal UserNode(string json) : base(json) {
        Status = GetStatus(GetString("status"));
    }

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