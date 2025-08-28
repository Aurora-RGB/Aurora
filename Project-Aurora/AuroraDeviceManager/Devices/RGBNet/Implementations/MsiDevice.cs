using AuroraDeviceManager.Utils;
using RGB.NET.Core;
using RGB.NET.Devices.Msi;

namespace AuroraDeviceManager.Devices.RGBNet.Implementations;

public class MsiDevice : RgbNetDevice
{
    public override string DeviceName => "MSI (RGB.NET)";

    protected override IRGBDeviceProvider Provider => MsiDeviceProvider.Instance;

    protected override Task ConfigureProvider(CancellationToken cancellationToken)
    {
        base.ConfigureProvider(cancellationToken);

        var relativePath = "./x64/MysticLight_SDK.dll";
        var absolutePath = Path.GetFullPath(relativePath);
        MsiDeviceProvider.PossibleX64NativePaths.Clear();
        MsiDeviceProvider.PossibleX64NativePaths.Add(absolutePath);

        var isMsiRunning = ProcessUtils.IsProcessRunning("Mystic_Light_Service.exe");
        if (!isMsiRunning)
        {
            throw new DeviceProviderException(new ApplicationException("MSI Mystic Light is not running! (Mystic_Light_Service.exe)"), false);
        }

        return Task.CompletedTask;
    }
}