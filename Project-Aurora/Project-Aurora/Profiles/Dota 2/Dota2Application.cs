using System.IO;
using System.Threading.Tasks;
using AuroraRgb.Profiles.Dota_2.Layers;
using AuroraRgb.Settings;
using AuroraRgb.Utils.Steam;

namespace AuroraRgb.Profiles.Dota_2;

public class Dota2 : GsiApplication
{
    public Dota2()
        : base(new LightEventConfig {
            Name = "Dota 2",
            ID = "dota2",
            AppID = "570",
            ProcessNames = ["dota2.exe"],
            SettingsType = typeof(NewJsonApplicationSettings),
            ProfileType = typeof(Dota2Profile),
            OverviewControlType = typeof(Control_Dota2),
            GameStateType = typeof(GSI.GameStateDota2),
            IconURI = "Resources/dota2_64x64.png"
        })
    {
        AllowLayer<Dota2BackgroundLayerHandler>();
        AllowLayer<Dota2RespawnLayerHandler>();
        AllowLayer<Dota2AbilityLayerHandler>();
        AllowLayer<Dota2ItemLayerHandler>();
        AllowLayer<Dota2HeroAbilityEffectsLayerHandler>();
        AllowLayer<Dota2KillstreakLayerHandler>();
    }

    protected override async Task<bool> DoInstallGsi()
    {
        return await SteamUtils.InstallGsiFile(
            570,
            Path.Combine("game", "dota", "cfg", "gamestate_integration", "gamestate_integration_aurora.cfg"),
            Properties.Resources.gamestate_integration_aurora_dota2
        );
    }
}