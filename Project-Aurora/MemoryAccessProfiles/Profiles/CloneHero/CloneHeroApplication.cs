using AuroraRgb.Profiles;
using AuroraRgb.Utils;
using MemoryAccessProfiles.Profiles.CloneHero.GSI;

namespace MemoryAccessProfiles.Profiles.CloneHero;

public class CloneHero : Application
{
    public CloneHero()
        : base(new LightEventConfig(() => new GameEvent_CloneHero())
        {
            Name = "Clone Hero", ID = "clonehero",
            ProcessNames = ["Clone Hero.exe"],
            ProfileType = typeof(CloneHeroProfile),
            OverviewControlType = typeof(Control_CloneHero),
            GameStateType = typeof(GameState_CloneHero),
            IconURI = "Resources/ch_128x128.png"
        })
    {
        PointerUpdateUtils.MarkAppForUpdate("CloneHero");
    }
}