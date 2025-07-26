namespace AuroraRgb.Profiles.OBS;

public class ObsApplication : Application
{
    public ObsApplication() : base(new LightEventConfig(() => new GameEventObs())
    {
        Name = "OBS Studio",
        ID = "ObsStudio",
        ProcessNames = ["obs64.exe", "obs32.exe"],
        ProfileType = typeof(ObsProfile),
        GameStateType = typeof(GameStateObs),
        OverviewControlType = typeof(ControlObs),
        IconURI = "Resources/obs.png",
        EnableByDefault = false,
    })
    {
    }
}