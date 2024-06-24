using System.IO;
using System.Threading.Tasks;
using AuroraRgb.Profiles.CSGO.Layers;
using AuroraRgb.Settings;
using AuroraRgb.Utils.Steam;

namespace AuroraRgb.Profiles.CSGO;

public class CSGO : GsiApplication
{
    public CSGO()
        : base(new LightEventConfig {
            Name = "CS2",
            ID = "csgo",
            AppID = "730",
            ProcessNames = ["csgo.exe", "cs2.exe"],
            SettingsType = typeof(NewJsonApplicationSettings),
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

    protected override async Task<bool> DoInstallGsi()
    {
        return await SteamUtils.InstallGsiFile(
            730,
            Path.Combine("game", "csgo", "cfg", "gamestate_integration_aurora.cfg"),
            Properties.Resources.gamestate_integration_aurora_csgo
        );
    }
}