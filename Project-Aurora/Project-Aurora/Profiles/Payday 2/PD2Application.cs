using AuroraRgb.Profiles.Payday_2.Layers;

namespace AuroraRgb.Profiles.Payday_2;

public class PD2 : Application
{
    public PD2()
        : base(new LightEventConfig {
            Name = "Payday 2",
            ID = "pd2",
            AppID= "218620",
            ProcessNames = new[] { "payday2_win32_release.exe" },
            ProfileType = typeof(PD2Profile),
            OverviewControlType = typeof(Pd2),
            GameStateType = typeof(GSI.GameState_PD2),
            IconURI = "Resources/pd2_64x64.png"
        })
    {
        AllowLayer<PD2BackgroundLayerHandler>();
        AllowLayer<PD2FlashbangLayerHandler>();
        AllowLayer<PD2StatesLayerHandler>();            
    }
}