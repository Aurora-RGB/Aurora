using AuroraDeviceManager;
using Common.Devices;
using AuroraDeviceManager.Devices;
using AuroraDeviceManager.Utils;
using Common;
using Common.Data;

SetShutdownPriority();

Global.Initialize();

AppDomain.CurrentDomain.UnhandledException += (_, eventArgs) =>
{
    Global.Logger.Fatal((Exception)eventArgs.ExceptionObject, "Device Manager crashed");
    if (Global.Logger is IDisposable logger)
    {
        logger.Dispose();
    }
};

var measurer = new PerformanceMeasurer();
Global.Logger.Information("Loading AuroraDeviceManager");
var deviceManager = new DeviceManager();
measurer.Measure("new DeviceManager()");

//Load config
Global.Logger.Information("Loading Configuration");
var configManager = new ConfigManager();
var loadConfigTask = configManager.Load(deviceManager);
measurer.Measure("= configManager.Load(deviceManager)");

var endTaskSource = new TaskCompletionSource();
var pipe = new AuroraPipe(deviceManager);
pipe.Shutdown += (_, _) => endTaskSource.TrySetResult();
measurer.Measure("new AuroraPipe(deviceManager)");

var colors = new MemorySharedArrayRead<SimpleColor>(Constants.DeviceLedMap);
var deviceKeys = new Dictionary<DeviceKeys, SimpleColor>();
measurer.Measure("new MemorySharedArray<SimpleColor>(Constants.DeviceLedMap)");
colors.Updated += OnColorsOnUpdated;

await loadConfigTask;
measurer.Measure("await loadConfigTask");
await deviceManager.InitializeDevices();
measurer.Measure("await deviceManager.InitializeDevices()");
measurer.Dispose();

await endTaskSource.Task;

Global.Logger.Information("Closing DeviceManager");
colors.Updated -= OnColorsOnUpdated;
Stop();
return;

void OnColorsOnUpdated(object? o, EventArgs eventArgs)
{
    for (var i = 0; i < colors.Count; i++)
    {
        var color = colors.ReadElement(i);
        deviceKeys[(DeviceKeys)i] = color;
    }

    deviceManager.UpdateDevices(deviceKeys);
}

void Stop()
{
    App.Closing = true;

    deviceManager.ShutdownDevices().Wait(5000);
    deviceManager.Dispose();

    endTaskSource.TrySetResult();
}

void SetShutdownPriority()
{
    const uint shutdownNotoriety = 0x00000001;
    //with this, DeviceManager shutdown will be called after Aurora, default priority is 280
    Kernel32.SetProcessShutdownParameters(0x210, shutdownNotoriety);
}
