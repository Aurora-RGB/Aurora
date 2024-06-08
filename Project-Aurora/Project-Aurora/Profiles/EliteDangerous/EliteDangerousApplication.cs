using AuroraRgb.Profiles.EliteDangerous.Layers;

namespace AuroraRgb.Profiles.EliteDangerous;

public class EliteDangerous : Application
{
    public EliteDangerous()
        : base(new LightEventConfig(() => new GameEvent_EliteDangerous())
        {
            Name = "Elite: Dangerous",
            ID = "EliteDangerous",
            ProcessNames = ["EliteDangerous64.exe"],
            SettingsType = typeof(EliteDangerousSettings),
            ProfileType = typeof(EliteDangerousProfile),
            OverviewControlType = typeof(Control_EliteDangerous),
            GameStateType = typeof(GSI.GameState_EliteDangerous),
            IconURI = "Resources/elite_dangerous_256x256.png"
        })
    {

        AllowLayer<EliteDangerousBackgroundLayerHandler>();
        AllowLayer<EliteDangerousKeyBindsLayerHandler>();
        AllowLayer<EliteDangerousAnimationLayerHandler>();
    }
}