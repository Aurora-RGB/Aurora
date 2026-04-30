namespace AuroraRgb.Nodes;

public class DevicesNode: Node
{
    public static readonly DevicesNode Instance = new();
    
    public Controllers Controllers { get; } = new();
    public RazerDevices RazerDevices { get; } = new();

    private DevicesNode()
    {
    }
}