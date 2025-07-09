using AuroraRgb.Settings;
using AuroraRgb.Settings.Layers;

namespace AuroraRgb.Profiles.DyingLight;

public class DyingLight : Application
{
    public DyingLight()
        : base(new LightEventConfig {
            Name = "Dying Light",
            ID = "DyingLight",
            ProcessNames = ["DyingLightGame.exe"],
            SettingsType = typeof(FirstTimeApplicationSettings),
            ProfileType = typeof(DyingLightProfile),
            OverviewControlType = typeof(Control_DyingLight),
            IconURI = "Resources/dl_128x128.png"
        })
    {
        AllowLayer<WrapperLightsLayerHandler>();
    }
}