namespace AuroraRgb.Profiles.Desktop;

public class Desktop() : Application(new LightEventConfig(() => new Event_Desktop())
{
    Name = "Desktop",
    ID = "desktop",
    ProfileType = typeof(DesktopProfile),
    OverviewControlType = typeof(Control_Desktop),
    GameStateType = typeof(DesktopState),
    IconURI = "Resources/desktop_icon.png"
});

public partial class DesktopState : GameState;