namespace AuroraRgb.Profiles.StardewValley;

public class StardewValley : Application {
    public StardewValley() : base(new LightEventConfig
    {
        Name = "Stardew Valley SMAPI",
        ID = "stardew_valley",
        AppID = "413150",
        ProcessNames = new[] { "StardewModdingAPI.exe" },
        ProfileType = typeof(StardewValleyProfile),
        OverviewControlType = typeof(Control_StardewValley),
        GameStateType = typeof(GSI.GameStateStardewValley),
        IconURI = "Resources/stardew_valley_32x32.png"
    })
    {
    }
}