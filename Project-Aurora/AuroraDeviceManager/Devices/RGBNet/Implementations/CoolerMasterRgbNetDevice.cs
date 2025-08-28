using RGB.NET.Core;
using RGB.NET.Devices.CoolerMaster;

namespace AuroraDeviceManager.Devices.RGBNet.Implementations;

public class CoolerMasterRgbNetDevice : RgbNetDevice
{
    protected override IRGBDeviceProvider Provider => CoolerMasterDeviceProvider.Instance;

    public override string DeviceName => "CoolerMaster (RGB.NET)";

    protected override Task ConfigureProvider(CancellationToken cancellationToken)
    {
        base.ConfigureProvider(cancellationToken);

        // normalise dll path
        var relativeDllPath = "x64/CMSDK.dll";
        var absoluteDllPath = Path.GetFullPath(relativeDllPath);
        CoolerMasterDeviceProvider.PossibleX64NativePaths.Clear();
        CoolerMasterDeviceProvider.PossibleX64NativePaths.Add(absoluteDllPath);

        return Task.CompletedTask;
    }
}