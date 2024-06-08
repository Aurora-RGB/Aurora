using AuroraRgb.Profiles.LeagueOfLegends.Layers;

namespace AuroraRgb.Profiles.LeagueOfLegends;

public class LoL : Application
{
    public LoL()
        : base(new LightEventConfig(() => new GameEvent_LoL()) { 
            Name = "League of Legends",
            ID = "league_of_legends",
            ProcessNames = ["league of legends.exe"],
            ProfileType = typeof(LoLGSIProfile),
            OverviewControlType = typeof(Control_LoL),
            GameStateType = typeof(GSI.GameState_LoL),
            IconURI = "Resources/leagueoflegends_48x48.png"
        })
    {
        AllowLayer<LoLBackgroundLayerHandler>();
    }
}