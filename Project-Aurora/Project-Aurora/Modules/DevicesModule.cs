using System.Threading.Tasks;
using AuroraRgb.Devices;
using RazerSdkReader;

namespace AuroraRgb.Modules;

public sealed class DevicesModule(Task<ChromaReader?> rzSdkManager, AuroraControlInterface auroraControlInterface) : AuroraModule
{
    public Task<DeviceManager> DeviceManager => _taskSource.Task;

    private readonly TaskCompletionSource<DeviceManager> _taskSource = new(TaskCreationOptions.RunContinuationsAsynchronously);
    private DeviceManager? _deviceManager;

    protected override async Task Initialize()
    {
        Global.logger.Information("Loading Device Manager...");

        _deviceManager = new DeviceManager(rzSdkManager, auroraControlInterface);
        _taskSource.SetResult(_deviceManager);

        await _deviceManager.InitializeDevices();
        Global.logger.Information("Loaded Device Manager");
    }

    public override async ValueTask DisposeAsync()
    {
        await _deviceManager?.ShutdownDevices()!;
        _deviceManager?.Dispose();
    }
}