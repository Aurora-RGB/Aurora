using System.Text.Json.Serialization;

namespace AuroraRgb.Profiles.Stationeers.GSI.Nodes;

public class WorldNode
{
    public static readonly WorldNode Default = new();

    [JsonPropertyName("planet")]
    public string Planet { get; set; } = string.Empty;

    [JsonPropertyName("tod")]
    public float Timeofday { get; set; }

}