using System.Collections.Generic;

namespace AuroraRgb.Modules.OnlineConfigs.Model;

public class RazerDevices
{
    public Dictionary<string, RazerMouseHidInfo> MouseHidInfos { get; set; } = new();
}

public class RazerMouseHidInfo
{
    public string Name { get; set; } = string.Empty;
    public string TransactionId { get; set; } = string.Empty;
}