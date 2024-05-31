using AuroraRgb.Profiles.CSGO.Layers;
using AuroraRgb.Settings;

namespace AuroraRgb.Profiles.CSGO;

public class CSGO : Application
{
    public CSGO()
        : base(new LightEventConfig {
            Name = "CS2",
            ID = "csgo",
            AppID = "730",
            ProcessNames = new[] { "csgo.exe", "cs2.exe" },
            SettingsType = typeof(FirstTimeApplicationSettings),
            ProfileType = typeof(CSGOProfile),
            OverviewControlType = typeof(Control_CSGO),
            GameStateType = typeof(GSI.GameStateCsgo),
            IconURI = "Resources/cs2.png"
        })
    {
        AllowLayer<CSGOKillIndicatorLayerHandler>();
        AllowLayer<CSGOBackgroundLayerHandler>();
        AllowLayer<CSGOBombLayerHandler>();
        AllowLayer<CSGOBurningLayerHandler>();
        AllowLayer<CSGOTypingIndicatorLayerHandler>();
        AllowLayer<CSGOWinningTeamLayerHandler>();
        AllowLayer<CSGODeathLayerHandler>();
    }
}