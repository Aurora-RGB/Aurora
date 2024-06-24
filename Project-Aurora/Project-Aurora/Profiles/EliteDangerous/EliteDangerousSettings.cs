using System.Text.Json.Serialization;
using AuroraRgb.Settings;

namespace AuroraRgb.Profiles.EliteDangerous;

[method: JsonConstructor]
public class EliteDangerousSettings(bool isFirstTimeInstalled) : FirstTimeApplicationSettings(isFirstTimeInstalled)
{
    public string GamePath { get; set; } = "";
}