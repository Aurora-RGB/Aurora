using System.Collections.Generic;
using System.IO;
using System.Text.Json.Serialization;
using Common.Devices;

namespace AuroraRgb.Modules.Gamebar;

public class GamebarConfig : IAuroraConfig
{
    public static readonly string ConfigFile = Path.Combine(Global.AppDataDirectory, "GamebarConfig.json");
    
    [JsonIgnore]
    public string ConfigPath => ConfigFile;
    
    public List<string> IgnoredPrograms { get; set; } = [];
}