namespace AuroraRgb.Profiles.Stationeers;

public class Stationeers : Application {

    public Stationeers() : base(new LightEventConfig
    {
        Name = "Stationeers",
        ID = "stationeers",
        AppID = "544550",
        ProcessNames = new[] { "rocketstation.exe" },
        ProfileType = typeof(StationeersProfile),
        OverviewControlType = typeof(Control_Stationeers),
        GameStateType = typeof(GSI.GameStateStationeers),
        IconURI = "Resources/Stationeers.png"
    })
    { }        
}