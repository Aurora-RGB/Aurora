namespace AuroraRgb.Profiles.Generic;

public class ProviderNode : AutoJsonNode<ProviderNode>
{
    public static readonly ProviderNode Default = new();

    public string Name { get; set; } = string.Empty;
    public int AppID { get; set; }

    public ProviderNode() { }

    internal ProviderNode(string json) : base(json) { }
}