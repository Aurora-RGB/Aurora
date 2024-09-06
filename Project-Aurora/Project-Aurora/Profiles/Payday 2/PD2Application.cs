using System.Threading.Tasks;
using AuroraRgb.Profiles.Payday_2.GSI;
using AuroraRgb.Profiles.Payday_2.Layers;
using AuroraRgb.Settings;

namespace AuroraRgb.Profiles.Payday_2;

public class PD2 : GsiApplication
{
    public PD2()
        : base(new LightEventConfig {
            Name = "Payday 2",
            ID = "pd2",
            AppID= "218620",
            ProcessNames = ["payday2_win32_release.exe"],
            SettingsType = typeof(FirstTimeApplicationSettings),
            ProfileType = typeof(PD2Profile),
            OverviewControlType = typeof(Pd2),
            GameStateType = typeof(GameState_PD2),
            IconURI = "Resources/pd2_64x64.png"
        })
    {
        AllowLayer<PD2BackgroundLayerHandler>();
        AllowLayer<PD2FlashbangLayerHandler>();
        AllowLayer<PD2StatesLayerHandler>();            
    }

    protected override async Task<bool> DoInstallGsi()
    {
        var errorMessage = await Pd2GsiUtils.InstallMod();
        return errorMessage == null;
    }
}