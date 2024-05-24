using System.Collections.Generic;
using System.IO;
using System.Text.Json.Serialization;
using AuroraRgb.Devices;
using Common.Devices;

namespace AuroraRgb.Modules.Razer;

public class AuroraChromaSettings : IAuroraConfig
{
    public static readonly string ConfigFile = Path.Combine(Global.AppDataDirectory, "ChromaConfig.json");
    
    [JsonIgnore]
    public string ConfigPath => ConfigFile;

    [JsonIgnore]
    public JsonSerializerLibrary JsonSerializerLibrary => JsonSerializerLibrary.SystemText;

    public List<string> DisabledApps { get; set; } = [DeviceManager.DeviceManagerExe, Global.AuroraExe];
}