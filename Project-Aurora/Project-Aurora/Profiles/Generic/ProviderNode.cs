using System.Text.Json.Serialization;

namespace AuroraRgb.Profiles.Generic;

public class ProviderNode : AutoJsonNode<ProviderNode>
{
    public static readonly ProviderNode Default = new();

    [JsonPropertyName("provider")]
    public string Name { get; set; } = string.Empty;
    
    [JsonPropertyName("appid")]
    public int AppID { get; set; }

    public ProviderNode() { }

    internal ProviderNode(string json) : base(json) { }
}