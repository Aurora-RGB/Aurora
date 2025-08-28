using AuroraDeviceManager.Utils;
using RGB.NET.Core;
using RGB.NET.Devices.Asus;

namespace AuroraDeviceManager.Devices.RGBNet.Implementations;

public class AsusDevice : RgbNetDevice
{
    public override string DeviceName => "Asus (RGB.NET)";

    protected override IRGBDeviceProvider Provider => AsusDeviceProvider.Instance;

    protected override Task ConfigureProvider(CancellationToken cancellationToken)
    {
        base.ConfigureProvider(cancellationToken);
        
        var isAuraRunning = ProcessUtils.IsProcessRunning("lightingservice");
        if (!isAuraRunning)
        {
            throw new DeviceProviderException(new ApplicationException("Aura or Armory Crate is not running! (LightingService.exe)"), false);
        }

        return Task.CompletedTask;
    }
}