using System.Collections.Immutable;
using System.Reflection;

namespace AuroraDeviceManager.Devices.ScriptedDevice;

internal sealed class DllDeviceLoader(string dllFolder) : IDeviceLoader
{
    private readonly HashSet<Assembly> _deviceAssemblies = [];

    public IEnumerable<IDevice> LoadDevices()
    {
        if (!Directory.Exists(dllFolder))
        {
            Directory.CreateDirectory(dllFolder);
            return ImmutableList<IDevice>.Empty;
        }

        var files = Directory.GetFiles(dllFolder, "*.dll");
        if (files.Length == 0)
        {
            Global.Logger.Information("No plugins found at {DllFolder}", dllFolder);
            return ImmutableList<IDevice>.Empty;
        }

        Global.Logger.Information("Loading devices plugins from {DllFolder}", dllFolder);

        AppDomain.CurrentDomain.AssemblyResolve += CurrentDomain_AssemblyResolve;

        var devices = new List<IDevice>();
        foreach (var deviceDll in files)
        {
            try
            {
                var deviceAssembly = Assembly.LoadFile(deviceDll);

                foreach (var type in deviceAssembly.GetExportedTypes())
                {
                    if (!typeof(IDevice).IsAssignableFrom(type) || type.IsAbstract) continue;
                    _deviceAssemblies.Add(deviceAssembly);
                    var devDll = (IDevice)Activator.CreateInstance(type);
                    Global.Logger.Information("Loaded device plugin {DeviceDll}", deviceDll);
                    devices.Add(devDll);
                }
            }
            catch (Exception e)
            {
                Global.Logger.Error(e, "Error loading device dll: {DeviceDll}", deviceDll);
            }
        }

        return devices;
    }

    private Assembly? CurrentDomain_AssemblyResolve(object? sender, ResolveEventArgs args)
    {
        
        if (args.RequestingAssembly == null || !_deviceAssemblies.Contains(args.RequestingAssembly)) return null;
        
        var searchDir = Path.GetDirectoryName(args.RequestingAssembly.Location);
        foreach (var file in Directory.GetFiles(searchDir, "*.dll"))
        {
            var assemblyName = AssemblyName.GetAssemblyName(file);
            if (assemblyName.FullName == args.Name)
            {
                return AppDomain.CurrentDomain.Load(assemblyName);
            }
        }
        
        foreach (var file in Directory.GetFiles(Path.Combine(searchDir, "x64"), "*.dll"))
        {
            var assemblyName = AssemblyName.GetAssemblyName(file);
            if (assemblyName.FullName == args.Name)
            {
                return AppDomain.CurrentDomain.Load(assemblyName);
            }
        }
        return null;
    }

    public void Dispose()
    {
    }
}