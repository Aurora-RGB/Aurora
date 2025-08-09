using Newtonsoft.Json;

namespace AuroraRgb.Profiles.Stationeers.GSI.Nodes;

public class Provider
{
    public static readonly Provider Default = new();

    public string Name { get; set; } = string.Empty;
    [JsonProperty("appid")]
    public int AppID { get; set; }
}