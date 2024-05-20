using System.Collections.Generic;
using System.IO;
using AuroraRgb.Devices;
using Common.Devices;

namespace AuroraRgb.Modules.Razer;

public class AuroraChromaSettings : IAuroraConfig
{
    public static readonly string ConfigFile = Path.Combine(Global.AppDataDirectory, "ChromaConfig.json");
    
    public string ConfigPath => ConfigFile;
    public List<string> DisabledApps { get; set; } = [DeviceManager.DeviceManagerExe, Global.AuroraExe];
}