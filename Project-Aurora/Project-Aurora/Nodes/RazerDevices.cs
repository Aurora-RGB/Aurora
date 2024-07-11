using AuroraRgb.Nodes.Razer;
using AuroraRgb.Utils;

namespace AuroraRgb.Nodes;

public class RazerDevices : Node
{
    private readonly Temporary<RazerBatteryPctFetcher> _razerBatteryFetcher = new(() => new RazerBatteryPctFetcher());

    private readonly Temporary<RazerBatteryStatusFetcher> _razerBatteryStatusFetcher = new(() => new RazerBatteryStatusFetcher());

    public double MouseBatteryPercentage => _razerBatteryFetcher.Value.MouseBatteryPercentage;
    public bool MouseBatteryCharging => _razerBatteryStatusFetcher.Value.MouseBatteryCharging;
}