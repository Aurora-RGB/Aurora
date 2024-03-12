﻿namespace AuroraRgb.Profiles.RocketLeague;

public class RocketLeague : Application
{
    public RocketLeague()
        : base(
            new LightEventConfig {
                Name = "Rocket League",
                ID = "rocketleague",
                ProcessNames = new[] { "rocketleague.exe" },
                ProfileType = typeof(RocketLeagueBMProfile),
                OverviewControlType = typeof(Control_RocketLeague),
                GameStateType = typeof(GSI.GameState_RocketLeague),
                IconURI = "Resources/rocketleague_256x256.png"
            })
    {
        AllowLayer<Layers.RocketLeagueGoalExplosionLayerHandler>();
    }
}