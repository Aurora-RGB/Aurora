using AuroraRgb.Nodes.Razer;
using AuroraRgb.Utils;

namespace AuroraRgb.Nodes;

public class RazerDevices : Node
{
    private Temporary<RazerBatteryFetcher> _razerBatteryFetcher = new(() => new RazerBatteryFetcher());

    public double MouseBatteryPercentage => _razerBatteryFetcher.Value.MouseBatteryPercentage;
}