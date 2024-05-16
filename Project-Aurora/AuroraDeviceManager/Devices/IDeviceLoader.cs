namespace AuroraDeviceManager.Devices;

internal interface IDeviceLoader : IDisposable
{
    IEnumerable<IDevice?> LoadDevices();
}

internal sealed class AssemblyDeviceLoader : IDeviceLoader
{
    public IEnumerable<IDevice> LoadDevices()
    {
        Global.Logger.Information("Loading devices from assembly...");
        return LoadFromGenerated();
    }

    private static IEnumerable<IDevice> LoadFromGenerated()
    {
        return from inst in DeviceSubTypes.GetInstances()
            orderby inst.DeviceName
            select inst;
    }

    public void Dispose()
    {
    }
}