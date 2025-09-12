using System.Collections.Generic;
using System.IO;
using System.Text.Json.Serialization;
using AuroraRgb.Devices;
using Common.Devices;

namespace AuroraRgb.Modules.Logitech;

public class AuroraLightsyncSettings : IAuroraConfig
{
    [JsonIgnore]
    public static readonly string ConfigFile = Path.Combine(Global.AppDataDirectory, "LightsyncConfig.json");
    
    [JsonIgnore]
    public string ConfigPath => ConfigFile;
    public List<string> DisabledApps { get; set; } = [Global.AuroraExe, DeviceManager.DeviceManagerExe];
}