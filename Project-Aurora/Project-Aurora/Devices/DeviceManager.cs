using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using AuroraRgb.EffectsEngine;
using AuroraRgb.Modules;
using AuroraRgb.Utils;
using Common;
using Common.Data;
using Common.Devices;

namespace AuroraRgb.Devices;

public sealed class DevicesUpdatedEventArgs(ImmutableList<DeviceContainer> deviceContainers) : EventArgs
{
    public ImmutableList<DeviceContainer> DeviceContainers { get; } = deviceContainers;
}

public sealed class DeviceManager : IDisposable
{
    private const string DeviceManagerProcess = "AuroraDeviceManager";

    private const string DeviceManagerFolder = @".\AuroraDeviceManager\";
    public const string DeviceManagerExe = "AuroraDeviceManager.exe";

    private bool _disposed;

    public event EventHandler<DevicesUpdatedEventArgs>? DevicesUpdated;

    public List<DeviceContainer> DeviceContainers { get; } = [];

    private readonly AuroraControlInterface _auroraControlInterface;
    private readonly MemorySharedArrayWrite<SimpleColor> _sharedDeviceColor;

    private int _dmStartCount;

    private readonly MemorySharedStruct<DeviceManagerInfo> _deviceManagerInfo;
    private Process? _process;
    private bool _detached;

    public DevicesPipe DevicesPipe { get; } = new();

    public DeviceManager(AuroraControlInterface auroraControlInterface)
    {
        _auroraControlInterface = auroraControlInterface;
        _sharedDeviceColor = new MemorySharedArrayWrite<SimpleColor>(Constants.DeviceLedMap, Effects.MaxDeviceId);

        _deviceManagerInfo = new MemorySharedStruct<DeviceManagerInfo>(Constants.DeviceInformations);
        _deviceManagerInfo.Updated += OnDeviceManagerInfoOnUpdated;
    }

    public Task InitializeDevices()
    {
        _dmStartCount = 0;

        AttachOrCreateProcess();
        return Task.CompletedTask;
    }

    private void AttachOrCreateProcess()
    {
        _process = Process.GetProcessesByName(DeviceManagerProcess).FirstOrDefault();
        if (_process != null)
        {
            UpdateDevices();
        }
        else
        {
            StartDmProcess();
        }
    }

    private void OnDeviceManagerInfoOnUpdated(object? o, EventArgs eventArgs)
    {
        UpdateDevices();
    }

    [MethodImpl(MethodImplOptions.Synchronized)]
    private void UpdateDevices()
    {
        var deviceManagerInfo = _deviceManagerInfo.ReadElement();
        var deviceNames = deviceManagerInfo.DeviceNames.Split(Constants.StringSplit);

        foreach (var deviceContainer in DeviceContainers)
        {
            deviceContainer.Dispose();
        }
        DeviceContainers.Clear();
        DeviceContainers.AddRange(deviceNames.Select(deviceName =>
        {
            var device = new MemorySharedDevice(deviceName);
            return new DeviceContainer(device, this);
        }));

        DevicesUpdated?.Invoke(this, new DevicesUpdatedEventArgs(DeviceContainers.ToImmutableList()));
    }

    private void StartDmProcess()
    {
        var updaterProc = new ProcessStartInfo
        {
            FileName = Path.Combine(DeviceManagerFolder, DeviceManagerExe),
            WorkingDirectory = DeviceManagerFolder,
            ErrorDialog = true,
        };
        _process = Process.Start(updaterProc);
        _process?.WaitForExitAsync().ContinueWith(DeviceManagerClosed);
    }

    private void DeviceManagerClosed(Task processTask)
    {
        if (processTask.IsFaulted)
        {
            Global.logger.Error(processTask.Exception, "Device Manager closed unexpectedly");
        }
        DevicesUpdated?.Invoke(this, new DevicesUpdatedEventArgs([]));

        if (_dmStartCount++ > 3)
        {
            Global.logger.Error("Device manager failed too much. Stopping initializing");
            _auroraControlInterface.ShowErrorNotification("Device Manager cannot be started. Check logs for more information");
            return;
        }

        if (_process != null)
        {
            AttachOrCreateProcess();
        }
    }

    public async Task ShutdownDevices()
    {
        if (_detached)
        {
            return;
        }
        _process ??= Process.GetProcessesByName(DeviceManagerProcess).FirstOrDefault();
        if (_process == null)
        {
            return;
        }

        var process = _process;
        _process = null;

        if (await IsDeviceManagerUp())
        {
            await DevicesPipe.Shutdown();
        }
        else
        {
            process.Kill();
        }

        using var processWaitTimeout = new CancellationTokenSource(1500);
        try
        {
            await process.WaitForExitAsync(processWaitTimeout.Token);
        }
        catch (TaskCanceledException)
        {
            process.Kill();
        }

        DevicesUpdated?.Invoke(this, new DevicesUpdatedEventArgs([]));
    }

    public async Task ResetDevices()
    {
        await ShutdownDevices();
        await InitializeDevices();
    }

    public void Detach()
    {
        _detached = true;
    }

    public void UpdateDevices(DeviceKeyStore keyColors)
    {
        _sharedDeviceColor.WriteArray(keyColors.ColorArray);
    }

    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }
        _disposed = true;
        _deviceManagerInfo.Dispose();
        _sharedDeviceColor.Dispose();
    }

    public async Task<bool> IsDeviceManagerUp()
    {
        var runningProcessMonitor = await ProcessesModule.RunningProcessMonitor;
        if (!runningProcessMonitor.IsProcessRunning(DeviceManagerExe.ToLowerInvariant()))
        {
            return false;
        }
        
        for (var i = 0; i < 5; i++)
        {
            var pipeOpen = await DevicesPipe.IsPipeOpen();
            if (pipeOpen)
            {
                return true;
            }
            await Task.Delay(200);
        }
        return false;
    }
}