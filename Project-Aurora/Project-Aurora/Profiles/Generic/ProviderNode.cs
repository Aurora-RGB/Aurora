namespace AuroraRgb.Profiles.Generic;

public class ProviderNode : AutoJsonNode<ProviderNode> {

    public string Name { get; set; } = string.Empty;
    public int AppID { get; set; }

    internal ProviderNode(string json) : base(json) { }
}