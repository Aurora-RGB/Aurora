using System;
using System.Threading.Tasks;

namespace AuroraRgb.Devices;

public sealed class DeviceContainer(MemorySharedDevice device, DeviceManager deviceManager) : IDisposable
{
    public MemorySharedDevice Device { get; } = device;

    public async Task EnableDevice()
    {
        await deviceManager.DevicesPipe.EnableController(Device.DeviceName);
    }

    public async Task DisableDevice()
    {
        await deviceManager.DevicesPipe.DisableController(Device.DeviceName);
    }

    public void Dispose()
    {
        Device.Dispose();
    }
}