using System.IO;
using System.Threading.Tasks;
using AuroraRgb.Utils.Steam;

namespace AuroraRgb.Profiles.RocketLeague;

public class RocketLeague : GsiApplication
{
    public RocketLeague()
        : base(new LightEventConfig(() => new RocketLeagueEvent())
        {
            Name = "Rocket League",
            ID = "rocketleague",
            ProcessNames = ["rocketleague.exe"],
            ProfileType = typeof(RocketLeagueBMProfile),
            SettingsType = typeof(RlSettings),
            OverviewControlType = typeof(Control_RocketLeague),
            GameStateType = typeof(GSI.GameStateRocketLeague),
            IconURI = "Resources/rocketleague_256x256.png"
        })
    {
        AllowLayer<Layers.RocketLeagueGoalExplosionLayerHandler>();
    }

    protected override async Task<bool> DoInstallGsi()
    {
        var gamePath = await SteamUtils.GetGamePathAsync(252950);
        if (string.IsNullOrEmpty(gamePath))
        {
            return false;
        }

        var iniPath = Path.Join(gamePath, "TAGame", "Config", "DefaultStatsAPI.ini");
        if (!File.Exists(iniPath))
        {
            return false;
        }

        if (Settings is not RlSettings rlSettings)
        {
            return false;
        }

        await RlStatsInstallUtils.EnableRlSocket(iniPath, rlSettings.SocketPort);
        return true;
    }
}