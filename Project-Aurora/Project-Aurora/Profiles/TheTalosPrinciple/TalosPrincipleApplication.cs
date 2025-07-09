using AuroraRgb.Settings;
using AuroraRgb.Settings.Layers;

namespace AuroraRgb.Profiles.TheTalosPrinciple;

public class TalosPrinciple : Application
{
    public TalosPrinciple()
        : base(new LightEventConfig {
            Name = "The Talos Principle",
            ID = "the_talos_principle",
            ProcessNames = ["talos.exe", "talos_unrestricted.exe"],
            SettingsType = typeof(FirstTimeApplicationSettings),
            ProfileType = typeof(TalosPrincipleProfile),
            OverviewControlType = typeof(Control_TalosPrinciple),
            IconURI = "Resources/talosprinciple_64x64.png"
        })
    {
        AllowLayer<WrapperLightsLayerHandler>();
    }
}