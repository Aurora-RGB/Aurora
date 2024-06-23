namespace AuroraRgb.Profiles.Discord;

public class Discord() : Application(new LightEventConfig
{
    Name = "Discord",
    ID = "discord",
    ProcessNames = ["Discord.exe", "DiscordPTB.exe", "DiscordCanary.exe"],
    ProfileType = typeof(DiscordProfile),
    OverviewControlType = typeof(Control_Discord),
    GameStateType = typeof(GSI.GameStateDiscord),
    IconURI = "Resources/betterdiscord.png",
    EnableByDefault = false
});