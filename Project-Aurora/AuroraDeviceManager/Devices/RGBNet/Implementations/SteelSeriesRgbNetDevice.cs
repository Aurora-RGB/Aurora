using AuroraDeviceManager.Utils;
using IronPython.Runtime.Operations;
using RGB.NET.Core;
using RGB.NET.Devices.SteelSeries;

namespace AuroraDeviceManager.Devices.RGBNet.Implementations;

public class SteelSeriesRgbNetDevice() : RgbNetDevice(true)
{
    private static readonly string SsEngineProcess = "SteelSeriesEngine".lower();
    private static readonly string SsGgProcess = "SteelSeriesGG".lower();

    private bool _sdkDetectedOff;

    protected override SteelSeriesDeviceProvider Provider => SteelSeriesDeviceProvider.Instance;

    public override string DeviceName => "SteelSeries (RGB.NET)";

    protected override async Task ConfigureProvider(CancellationToken cancellationToken)
    {
        await base.ConfigureProvider(cancellationToken);

        var isSteelGgRunning = ProcessUtils.IsProcessRunning(SsEngineProcess);
        var isSteelEngineRunning = ProcessUtils.IsProcessRunning(SsGgProcess);

        if (!(isSteelGgRunning && isSteelEngineRunning))
        {
            _sdkDetectedOff = true;
            throw new DeviceProviderException(new ApplicationException("SteelSeries Engine is not running!"), false);
        }

        if (_sdkDetectedOff)
        {
            await Task.Delay(5000, cancellationToken);
        }

        _sdkDetectedOff = false;
    }
}