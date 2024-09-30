using System.Text.Json.Serialization;
using AuroraRgb.Settings;

namespace AuroraRgb.Profiles.EliteDangerous;


public class EliteDangerousSettings : FirstTimeApplicationSettings
{
    public EliteDangerousSettings()
    {
    }

    [method: JsonConstructor]
    public EliteDangerousSettings(bool isFirstTimeInstalled) : base(isFirstTimeInstalled)
    {
    }

    public string GamePath { get; set; } = "";
}