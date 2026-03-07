using AuroraRgb.Profiles.Discord.Layers;

namespace AuroraRgb.Profiles.Discord;

public class Discord : Application
{
    public Discord() : base(new LightEventConfig
    {
        Name = "Discord",
        ID = "discord",
        ProcessNames = ["Discord.exe", "DiscordPTB.exe", "DiscordCanary.exe"],
        ProfileType = typeof(DiscordProfile),
        OverviewControlType = typeof(Control_Discord),
        GameStateType = typeof(GSI.GameStateDiscord),
        IconURI = "Resources/betterdiscord.png",
        EnableByDefault = false
    })
    {
        AllowLayer<DiscordVoiceActivityLayerHandler>();
    }
}