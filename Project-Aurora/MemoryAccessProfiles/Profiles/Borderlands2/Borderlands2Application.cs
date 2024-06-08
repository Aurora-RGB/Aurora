using AuroraRgb.Profiles;
using AuroraRgb.Utils;
using MemoryAccessProfiles.Profiles.Borderlands2.GSI;

namespace MemoryAccessProfiles.Profiles.Borderlands2;

public class Borderlands2 : Application
{
    public Borderlands2()
        : base(new LightEventConfig(() => new GameEvent_Borderlands2())
        {
            Name = "Borderlands 2",
            ID = "borderlands2",
            ProcessNames = ["borderlands2.exe"],
            ProfileType = typeof(Borderlands2Profile),
            OverviewControlType = typeof(Control_Borderlands2),
            GameStateType = typeof(GameState_Borderlands2),
            IconURI = "Resources/Borderlands2_256x256.png"
        })
    {
        PointerUpdateUtils.MarkAppForUpdate("Borderlands2");
    }
}