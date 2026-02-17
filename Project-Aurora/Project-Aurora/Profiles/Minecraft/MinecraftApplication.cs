using System.Text.RegularExpressions;
using AuroraRgb.Profiles.Minecraft.Layers;

namespace AuroraRgb.Profiles.Minecraft;

public partial class Minecraft : Application {

    public Minecraft() : base(new LightEventConfig {
        Name = "Minecraft",
        ID = "minecraft",
        ProcessNames = ["javaw.exe", "java.exe"], // Require the process to a Java application
        ProcessTitles = [StartsWithMinecraft()], // Match any window title that starts with Minecraft
        ProfileType = typeof(MinecraftProfile),
        OverviewControlType = typeof(Control_Minecraft),
        GameStateType = typeof(GSI.GameStateMinecraft),
        IconURI = "Resources/minecraft_128x128.png"
    }) {
        AllowLayer<MinecraftBackgroundLayerHandler>();
        AllowLayer<MinecraftKeyConflictLayerHandler>();
    }

    [GeneratedRegex(@"^Minecraft")]
    private static partial Regex StartsWithMinecraft();
}