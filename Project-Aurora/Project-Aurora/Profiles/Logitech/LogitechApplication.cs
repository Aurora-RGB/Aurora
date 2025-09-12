using System.IO;
using AuroraRgb.Modules;

namespace AuroraRgb.Profiles.Logitech;

public class LogitechApplication : Application
{
    public LogitechApplication() : base(new LightEventConfig
    {
        Name = "Logitech Lightsync",
        ID = "logitech",
        ProcessNames = [],
        ProfileType = typeof(LogitechProfile),
        OverviewControlType = typeof(Control_Logitech),
        IconURI = "Resources/G-sync.png",
        EnableByDefault = true,
        Priority = 4,
    })
    {
        LogitechSdkModule.LogitechSdkListener.ApplicationChanged += LogitechSdkListenerOnApplicationChanged;
    }

    private void LogitechSdkListenerOnApplicationChanged(object? sender, string? e)
    {
        Config.ProcessNames = e == null ? [] : [Path.GetFileName(e)];
    }
}