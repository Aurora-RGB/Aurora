using AuroraRgb.Nodes.Razer;
using AuroraRgb.Profiles;
using AuroraRgb.Utils;

namespace AuroraRgb.Nodes;

public class RazerDevices : Node
{
    private RazerMouseNode? _mouse;
    public RazerMouseNode Mouse => _mouse ??= new RazerMouseNode();
}

[GameStateDescription(Description)]
public class RazerMouseNode : Node
{
    private const string Description = """
                                       Not compatible with running in tandem with Synapse.
                                       Compatible with OpenRGB 2025 and later builds
                                       """;

    private readonly Temporary<RazerBatteryPctFetcher> _razerBatteryFetcher = new(() => new RazerBatteryPctFetcher());

    private readonly Temporary<RazerBatteryStatusFetcher> _razerBatteryStatusFetcher = new(() => new RazerBatteryStatusFetcher());

    public double BatteryPercentage => _razerBatteryFetcher.Value.MouseBatteryPercentage;
    public bool BatteryCharging => _razerBatteryStatusFetcher.Value.MouseBatteryCharging;
}