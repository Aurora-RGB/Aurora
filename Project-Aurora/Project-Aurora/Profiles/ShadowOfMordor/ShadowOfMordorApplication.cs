﻿using AuroraRgb.Settings;
using AuroraRgb.Settings.Layers;

namespace AuroraRgb.Profiles.ShadowOfMordor;

public class ShadowOfMordor : Application
{
    public ShadowOfMordor()
        : base(new LightEventConfig {
            Name = "Middle-earth: Shadow of Mordor",
            ID = "ShadowOfMordor",
            ProcessNames = new[] { "shadowofmordor.exe" },
            SettingsType = typeof(FirstTimeApplicationSettings),
            ProfileType = typeof(ShadowOfMordorProfile),
            OverviewControlType = typeof(Control_ShadowOfMordor),
            GameStateType = typeof(GameState_Wrapper),
            IconURI = "Resources/shadow_of_mordor_64x64.png"
        })
    {
        AllowLayer<WrapperLightsLayerHandler>();
    }
}