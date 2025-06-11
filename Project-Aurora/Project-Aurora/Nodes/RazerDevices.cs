using AuroraRgb.Nodes.Razer;
using AuroraRgb.Utils;

namespace AuroraRgb.Nodes;

public class RazerDevices : Node
{
    private RazerMouseNode? _mouse;
    public RazerMouseNode Mouse => _mouse ??= new RazerMouseNode();
}

public class RazerMouseNode : Node
{
    private readonly Temporary<RazerBatteryPctFetcher> _razerBatteryFetcher = new(() => new RazerBatteryPctFetcher());

    private readonly Temporary<RazerBatteryStatusFetcher> _razerBatteryStatusFetcher = new(() => new RazerBatteryStatusFetcher());

    public double BatteryPercentage => _razerBatteryFetcher.Value.MouseBatteryPercentage;
    public bool BatteryCharging => _razerBatteryStatusFetcher.Value.MouseBatteryCharging;
}