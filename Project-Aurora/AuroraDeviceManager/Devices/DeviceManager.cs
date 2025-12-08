using System.IO.Pipes;
using System.Text;
using System.Text.Json;
using AuroraDeviceManager.Devices.RGBNet;
using AuroraDeviceManager.Devices.ScriptedDevice;
using AuroraDeviceManager.Utils;
using Common;
using Common.Data;
using Common.Devices;
using Common.Devices.RGBNet;
using Microsoft.Win32;
using RGB.NET.Core;
using RgbNetColor = RGB.NET.Core.Color;

namespace AuroraDeviceManager.Devices;

public sealed class DeviceManager : IDisposable
{
    private bool _suspended;
    private bool _disposed;
    private readonly MemorySharedStruct<DeviceManagerInfo> _deviceInformations;

    private List<DeviceContainer> DeviceContainers { get; } = [];

    private IEnumerable<DeviceContainer> InitializedDeviceContainers =>
        DeviceContainers.Where(d => d.Device.IsInitialized);

    public DeviceManager()
    {
        const string devicesPath = "Devices";
        var deviceLoaders = new IDeviceLoader[]
        {
            new AssemblyDeviceLoader(),
            new ScriptDeviceLoader(Path.Combine(Global.ExecutingDirectory, "Scripts", devicesPath)),
            new ScriptDeviceLoader(Path.Combine(Global.AppDataDirectory, "Scripts", devicesPath)),
            new DllDeviceLoader(Path.Combine(Global.ExecutingDirectory, "Plugins", devicesPath)),
            new DllDeviceLoader(Path.Combine(Global.AppDataDirectory, "Plugins", devicesPath))
        };

        foreach (var deviceLoader in deviceLoaders)
        {
            var enumerable = deviceLoader.LoadDevices()
                .Where(d => d != null)
                .Select(d => new DeviceContainer(d!));
            DeviceContainers.AddRange(enumerable);

            deviceLoader.Dispose();
        }

        _deviceInformations = new MemorySharedStruct<DeviceManagerInfo>(Constants.DeviceInformations, CreateDeviceManagerInfo());

        UpdateSharedDeviceInfos();
        _deviceInformations.UpdateRequested += (_, _) => { UpdateSharedDeviceInfos(); };

        SystemEvents.PowerModeChanged += SystemEvents_PowerModeChanged;
        
        Task.Run(ShareRemappableDevices);
    }

    private void UpdateSharedDeviceInfos()
    {
        Global.Logger.Information("Updating device infos");
        var a = CreateDeviceManagerInfo();
        _deviceInformations.WriteObject(a);
    }

    private DeviceManagerInfo CreateDeviceManagerInfo()
    {
        var devices = string.Join('~', DeviceContainers
            .Select(dc => dc.Device)
            .Select(d => d.DeviceName));

        return new DeviceManagerInfo(devices);
    }

    public void RegisterVariables()
    {
        foreach (var device in DeviceContainers)
        {
            device.UpdateVariables();
        }
    }

    public async Task InitializeDevices()
    {
        if (_suspended)
            return;

        var initializeTasks = DeviceContainers
            .Where(dc => dc.Device is { IsDoingWork: false })
            .Where(dc => dc.Device.IsInitialized ^ DeviceEnabled(dc))
            .Select(deviceContainer => deviceContainer.Device.IsInitialized
                ? deviceContainer.Disable()
                : deviceContainer.Enable())
            .ToList();

        if (!initializeTasks.Any())
            return;
        
        await Task.WhenAny(initializeTasks);
    }

    private static bool DeviceEnabled(DeviceContainer dc)
    {
        return Global.DeviceConfig.EnabledControllers.Contains(dc.Device.DeviceName);
    }

    public Task ShutdownDevices()
    {
        var startingDevices = DeviceContainers.Where(dc => dc.Device is { IsInitialized: false, IsDoingWork: true });
        var shutdownTasks = InitializedDeviceContainers.Union(startingDevices).Select(dc => dc.Disable());

        return Task.WhenAll(shutdownTasks);
    }

    public void UpdateDevices(Dictionary<DeviceKeys, SimpleColor> composition)
    {
        if (_disposed)
            return;
        foreach (var dc in InitializedDeviceContainers)
        {
            dc.UpdateDevice(composition);
        }
    }

    #region SystemEvents

    private async void SystemEvents_PowerModeChanged(object? sender, PowerModeChangedEventArgs e)
    {
        switch (e.Mode)
        {
            case PowerModes.Suspend:
                Global.Logger.Information("Suspending Devices");
                _suspended = true;
                await Task.Run(async () => await ShutdownDevices());
                break;
            case PowerModes.Resume:
                Global.Logger.Information("Resuming Devices -- PowerModes.Resume");
                Thread.Sleep(TimeSpan.FromSeconds(5));
                _suspended = false;
                await Task.Run(async () => await InitializeDevices());
                break;
        }
    }

    #endregion

    public void Dispose()
    {
        _disposed = true;
        foreach (var deviceContainer in DeviceContainers)
        {
            deviceContainer.Dispose();
        }
        DeviceContainers.Clear();
    }

    private CancellationTokenSource? _tokenSource;
    private const int BlinkCount = 7;

    public void BlinkDevice(string deviceId, LedId led)
    {
        foreach (var auroraDevice in InitializedDeviceContainers.Select(container => container.Device).OfType<RgbNetDevice>())
        {
            foreach (var rgbNetDevice in auroraDevice.DeviceList.Where(rgbDevice => rgbDevice.DeviceInfo.DeviceName == deviceId))
            {
                _tokenSource?.Cancel();
                auroraDevice.Disabled = true;
                BlinkKey(rgbNetDevice, led)
                    .ContinueWith(_ =>
                        {
                            auroraDevice.Disabled = false;
                            return Task.CompletedTask;
                        }
                    );
            }
        }
    }

    private async Task BlinkKey(IRGBDevice device, LedId ledId)
    {
        if (_tokenSource is { Token.IsCancellationRequested: false })
            await _tokenSource.CancelAsync();

        var led = device[ledId];
        if (led == null)
        {
            return;
        }

        _tokenSource?.Dispose();
        _tokenSource = new CancellationTokenSource();
        var token = _tokenSource.Token;
        try
        {
            for (var i = 0; i < BlinkCount; i++)
            {
                if (token.IsCancellationRequested)
                    return;

                // set everything to black
                foreach (var light in device)
                    light.Color = new RgbNetColor(0, 0, 0);

                // set this one key to white
                if (i % 2 == 1)
                {
                    led.Color = new RgbNetColor(255, 255, 255);
                }

                device.Update();
                await Task.Delay(200, token); // ms
            }
        }
        catch (Exception e)
        {
            Global.Logger.Error(e, "Error while blinking device led");
        }
    }

    public async Task ShareRemappableDevices()
    {
        Global.Logger.Information("Updating CurrentDevices.json");
        var rgbNetControllers = InitializedDeviceContainers.Select(dc => dc.Device).OfType<RgbNetDevice>();

        var remappableDevices = (
            from rgbNetController in rgbNetControllers
            from device in rgbNetController.DeviceList
            let calibration = Global.DeviceConfig.DeviceCalibrations.GetValueOrDefault(device.DeviceInfo.DeviceName, SimpleColor.White)
            let deviceSummary = $"{rgbNetController.DeviceName}: [{device.DeviceInfo.DeviceType}] {device.DeviceInfo.DeviceName}"
            let rgbNetLeds = device.Select(l => l.Id).ToList()
            select new RemappableDevice(device.DeviceInfo.DeviceName, deviceSummary, rgbNetLeds, calibration, !rgbNetController.NeedsLayout())
        ).ToList();

        var currentDevices = new CurrentDevices(remappableDevices);

        var json = JsonSerializer.Serialize(currentDevices, SourceGenerationContext.Default.CurrentDevices);
        var command = DeviceCommands.RemappableDevices + Constants.StringSplit + json;

        await SendCommand(command);
    }

    public void RemapKey(string deviceId, LedId deviceLed, DeviceKeys? remappedKey)
    {
        var rgbNetControllers = InitializedDeviceContainers.Select(dc => dc.Device).OfType<RgbNetDevice>();
        
        foreach (var rgbNetDevice in rgbNetControllers)
        foreach (var device in rgbNetDevice.DeviceList.Where(device => device.DeviceInfo.DeviceName == deviceId))
        {
            if (!rgbNetDevice.DeviceKeyRemap.TryGetValue(device, out var deviceKeyMap))
            {
                deviceKeyMap = new Dictionary<LedId, DeviceKeys>();
                rgbNetDevice.DeviceKeyRemap[device] = deviceKeyMap;
            }

            var led = device[deviceLed];
            if (led == null)
            {
                //in case somehow this device doesn't have this led
                Global.Logger.Warning("Device {DeviceId} does not have led {LedId}", deviceId, deviceLed);
                continue;
            }
            
            var deviceRemap = GetDeviceRemap(device);
            if (remappedKey == null)
            {
                deviceKeyMap.Remove(led.Id);
                deviceRemap.KeyMapper.Remove(led.Id);
            }
            else
            {
                deviceKeyMap[led.Id] = (DeviceKeys)remappedKey;
                deviceRemap.KeyMapper[led.Id] = (DeviceKeys)remappedKey;
            }
        }
    }

    private DeviceRemap GetDeviceRemap(IRGBDevice device)
    {
        var remaps = DeviceMappingConfig.Config.Devices
            .Find(netConfigDevice => netConfigDevice.Name.Equals(device.DeviceInfo.DeviceName));
        if (remaps != null)
        {
            return remaps;
        }

        var rgbNetConfigDevice = new DeviceRemap(device.DeviceInfo.DeviceName);
        DeviceMappingConfig.Config.Devices.Add(rgbNetConfigDevice);
        return rgbNetConfigDevice;
    }

    public async Task Enable(string deviceController)
    {
        await DeviceContainers.First(dc => dc.Device.DeviceName == deviceController).Enable();
    }

    public async Task Disable(string deviceController)
    {
        await DeviceContainers.First(dc => dc.Device.DeviceName == deviceController).Disable();
    }

    public async Task EnableDevice(string deviceController, string deviceId)
    {
        await DeviceContainers.First(dc => dc.Device.DeviceName == deviceController).EnableDevice(deviceId);
    }

    public async Task DisableDevice(string deviceController, string deviceId)
    {
        await DeviceContainers.First(dc => dc.Device.DeviceName == deviceController).DisableDevice(deviceId);
    }

    private readonly byte[] _end = "\n"u8.ToArray();

    private async Task SendCommand(string command)
    {
        await SendCommand(Encoding.UTF8.GetBytes(command));
    }

    private async Task SendCommand(byte[] command)
    {
        var client = new NamedPipeClientStream(".", Constants.AuroraInterfacePipe, PipeDirection.Out, PipeOptions.Asynchronous);
        await client.ConnectAsync(2000);
        if (!client.IsConnected)
            return;
        
        await client.WriteAsync(command, 0, command.Length);
        await client.WriteAsync(_end, 0, _end.Length);
        
        await client.FlushAsync();
        client.Close();
    }
}