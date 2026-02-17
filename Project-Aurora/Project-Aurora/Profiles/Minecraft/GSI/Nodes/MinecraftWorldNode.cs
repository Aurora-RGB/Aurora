using System.Text.Json.Serialization;

namespace AuroraRgb.Profiles.Minecraft.GSI.Nodes;

public class MinecraftWorldNode
{
    public static MinecraftWorldNode Default => new();
    
    [JsonPropertyName("worldTime")]
    public long WorldTime { get; set; }
    [JsonPropertyName("isDayTime")]
    public bool IsDayTime { get; set; }
    [JsonPropertyName("isRaining")]
    public bool IsRaining { get; set; }
    [JsonPropertyName("rainStrength")]
    public float RainStrength { get; set; }
    [JsonPropertyName("dimensionID")]
    public int DimensionId { get; set; }
}