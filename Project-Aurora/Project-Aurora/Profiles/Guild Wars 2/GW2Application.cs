using AuroraRgb.Settings;
using AuroraRgb.Settings.Layers;

namespace AuroraRgb.Profiles.Guild_Wars_2;

public class GW2 : Application
{
    public GW2() : base(new LightEventConfig {
        Name = "Guild Wars 2",
        ID = "GW2",
        ProcessNames = ["gw2.exe", "gw2-64.exe"],
        SettingsType = typeof(FirstTimeApplicationSettings),
        ProfileType = typeof(GW2Profile),
        OverviewControlType = typeof(Control_GW2),
        IconURI = "Resources/gw2_48x48.png"
    })
    {
        AllowLayer<WrapperLightsLayerHandler>();
    }
}