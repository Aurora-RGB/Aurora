namespace AuroraRgb.Nodes;

public class DevicesNode: Node
{
    public Controllers Controllers { get; } = new();
    public RazerDevices RazerDevices { get; } = new();
}