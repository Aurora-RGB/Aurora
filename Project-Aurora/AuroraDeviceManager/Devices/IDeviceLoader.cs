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
        return from type in DeviceSubTypes.GetSubTypes()
            where type != typeof(ScriptedDevice.ScriptedDevice)
                  && type.GetConstructor(Type.EmptyTypes) != null
            let inst = (IDevice)Activator.CreateInstance(type)
            orderby inst.DeviceName
            select inst;
    }

    public void Dispose()
    {
    }
}