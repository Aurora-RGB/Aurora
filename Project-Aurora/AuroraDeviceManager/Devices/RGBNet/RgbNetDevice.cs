using System.Collections.Concurrent;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using Common;
using Common.Devices;
using Common.Devices.RGBNet;
using RGB.NET.Core;

namespace AuroraDeviceManager.Devices.RGBNet;

public abstract class RgbNetDevice : DefaultDevice
{
    private readonly bool _needsLayout;
    public bool Disabled { get; set; }
    public ConcurrentDictionary<IRGBDevice, Dictionary<LedId, DeviceKeys>> DeviceKeyRemap { get; } = new();
    public readonly IList<IRGBDevice> DeviceList = [];

    protected override string DeviceInfo => ErrorMessage ?? GetDeviceStatus();
    protected abstract IRGBDeviceProvider Provider { get; }

    private readonly Dictionary<string, int> _deviceCountMap = new();
    private string? ErrorMessage { get; set; }
    private readonly RgbNetDeviceUpdater _updater;
    private string? _devicesString;

    protected RgbNetDevice()
    {
        _updater = new RgbNetDeviceUpdater(DeviceKeyRemap, false);
    }

    protected RgbNetDevice(bool needsLayout)
    {
        _needsLayout = needsLayout;
        _updater = new RgbNetDeviceUpdater(DeviceKeyRemap, needsLayout);
    }

    private string GetDeviceStatus()
    {
        if (!IsInitialized)
            return "";

        if (!Provider.Devices.Any())
            return "Initialized: No devices connected";

        return "Initialized: " + string.Join(", ", _deviceCountMap.Select(pair => pair.Value > 1 ? pair.Key + " x" + pair.Value : pair.Key));
    }

    public override string? GetDevices()
    {
        return _devicesString;
    }

    protected override async Task<bool> DoInitialize(CancellationToken cancellationToken)
    {
        Global.Logger.Information("Initializing {DeviceName}", DeviceName);

        var connectSleepTimeSeconds = Global.DeviceConfig.VarRegistry.GetVariable<int>($"{DeviceName}_connect_timeout");
        var remainingMillis = TimeSpan.FromSeconds(connectSleepTimeSeconds);

        Provider.DevicesChanged += ProviderOnDevicesChanged;
        var timeWatch = Stopwatch.StartNew();
        do
        {
            try
            {
                timeWatch.Stop();
                await ConfigureProvider(cancellationToken);
                timeWatch.Start();

                Provider.Initialize(RGBDeviceType.All, true);

                if (!HotplugEnabled() && DeviceList.Count == 0)
                {
                    await Task.Delay(1500, cancellationToken);
                    const string message = "No device detected yet!";
                    ErrorMessage = message;
                    throw new DeviceProviderException(new ApplicationException(message), false);
                }

                IsInitialized = true;
                ErrorMessage = null;
                OnInitialized();
                Provider.Exception += ProviderOnException;
                return true;
            }
            catch (DeviceProviderException e)
            {
                Global.Logger.Error(e, "Device {DeviceProvider} init threw exception", DeviceName);
                remainingMillis -= timeWatch.Elapsed;
                timeWatch.Restart();

                if (e.IsCritical || remainingMillis <= TimeSpan.Zero)
                {
                    if (remainingMillis <= TimeSpan.Zero)
                    {
                        Global.Logger.Error("{DeviceProvider} initialization timed out after {Timeout} seconds",
                            DeviceName, connectSleepTimeSeconds);
                    }

                    ErrorMessage = $"{e.Message}";
                    Provider.DevicesChanged -= ProviderOnDevicesChanged;
                    return false;
                }

                ErrorMessage = $"{e.Message} ({remainingMillis.Seconds.ToString()})";

                try
                {
                    await Task.Delay(3800, cancellationToken);
                }
                catch (TaskCanceledException)
                {
                    return false;
                }
            }
            catch (TaskCanceledException)
            {
                return false;
            }
        } while (!cancellationToken.IsCancellationRequested);

        return false;
    }

    private async void ProviderOnException(object? sender, ExceptionEventArgs e)
    {
        Global.Logger.Error(e.Exception, "Device provider {DeviceProvider} threw exception", DeviceName);
        await Reset();
    }

    private void ProviderOnDevicesChanged(object? sender, DevicesChangedEventArgs e)
    {
        lock (DeviceList)
        {
            switch (e.Action)
            {
                case DevicesChangedEventArgs.DevicesChangedAction.Added:
                    ProviderOnDeviceAdded(e.Device);
                    break;
                case DevicesChangedEventArgs.DevicesChangedAction.Removed:
                    ProviderOnDeviceRemoved(e.Device);
                    break;
            }

            _devicesString = string.Join(Constants.StringSplit, DeviceList.Select(device => device.DeviceInfo.DeviceName));
        }
    }

    private void ProviderOnDeviceRemoved(IRGBDevice device)
    {
        DeviceList.Remove(device);

        var deviceName = device.DeviceInfo.Manufacturer + " " + device.DeviceInfo.Model;
        _deviceCountMap.TryGetValue(deviceName, out var count);
        if (--count == 0)
        {
            _deviceCountMap.Remove(deviceName);
        }
        else
        {
            _deviceCountMap[deviceName] = count;
        }
        Global.Logger.Information("[{DeviceType}] Device removed: {DeviceName}", DeviceName, deviceName);
    }

    private void ProviderOnDeviceAdded(IRGBDevice device)
    {
        DeviceList.Add(device);

        var deviceName = device.DeviceInfo.Manufacturer + " " + device.DeviceInfo.Model;
        if (_deviceCountMap.TryGetValue(deviceName, out var count))
        {
            _deviceCountMap[deviceName] = count + 1;
        }
        else
        {
            _deviceCountMap.Add(deviceName, 1);
        }
        Global.Logger.Information("[{DeviceType}] Device added: {DeviceName}", DeviceName, deviceName);

        Task.Run(() =>
        {
            var rgbNetConfigDevices =
                DeviceMappingConfig.Config.Devices.ToDictionary(device1 => device1.Name, device2 => device2);
            RemapDeviceKeys(rgbNetConfigDevices, device);
        });
    }

    private void RemapDeviceKeys(Dictionary<string, DeviceRemap> rgbNetConfigDevices, IRGBDevice rgbDevice)
    {
        if (rgbNetConfigDevices.TryGetValue(rgbDevice.DeviceInfo.DeviceName, out var configDevice))
        {
            DeviceKeyRemap.TryAdd(rgbDevice, configDevice.KeyMapper);
        }
    }

    [MethodImpl(MethodImplOptions.Synchronized)]
    protected override Task Shutdown()
    {
        if (!IsInitialized)
        {
            return Task.CompletedTask;
        }
        if (!OnShutdown())
        {
            return Task.CompletedTask;
        }

        if (Provider.IsInitialized)
        {
            Provider.Dispose();
        }

        Provider.DevicesChanged -= ProviderOnDevicesChanged;

        IsInitialized = false;
        return Task.CompletedTask;
    }

    public bool NeedsLayout()
    {
        return _needsLayout;
    }

    public virtual bool HotplugEnabled()
    {
        return false;
    }

    protected virtual Task ConfigureProvider(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }

    protected virtual void OnInitialized()
    {
    }

    /// <summary>
    /// Do shutdown tasks
    /// </summary>
    /// <returns>Whether shutdown should continue</returns>
    protected virtual bool OnShutdown()
    {
        return true;
    }

    protected override Task<bool> UpdateDevice(Dictionary<DeviceKeys, SimpleColor> keyColors, DoWorkEventArgs e,
        bool forced = false)
    {
        if (Disabled) return Task.FromResult(false);
        lock (DeviceList)
        {
            foreach (var device in DeviceList)
            {
                _updater.UpdateDevice(keyColors, device);
            }
        }

        return Task.FromResult(true);
    }

    protected override void RegisterVariables(VariableRegistry variableRegistry)
    {
        base.RegisterVariables(variableRegistry);

        variableRegistry.Register($"{DeviceName}_connect_timeout", 200, "Connection timeout seconds");
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);

        if (IsInitialized)
        {
            ShutdownDevice().Wait();
        }
    }
}