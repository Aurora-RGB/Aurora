using Newtonsoft.Json;

namespace AuroraRgb.Profiles.Stationeers.GSI.Nodes;

public class WorldNode
{
    public static readonly WorldNode Default = new();

    public string Planet { get; set; } = string.Empty;
    [JsonProperty("timeofday")]
    public float TimeOfDay { get; set; }
    [JsonProperty("planetpressure")]
    public int PlanetPressure { get; set; }
    [JsonProperty("planettemperature")]
    public int PlanetTemperature { get; set; } 
}