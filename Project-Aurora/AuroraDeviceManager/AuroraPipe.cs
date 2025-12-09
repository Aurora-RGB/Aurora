using System.IO.Pipes;
using System.Security.AccessControl;
using System.Security.Principal;
using AuroraDeviceManager.Devices;
using AuroraDeviceManager.Utils;
using Common;
using Common.Devices;
using RGB.NET.Core;

namespace AuroraDeviceManager;

public sealed class AuroraPipe : IDisposable
{
    public event EventHandler<EventArgs>? Shutdown;

    private readonly DeviceManager _deviceManager;

    private readonly List<NamedPipeServerStream> _pipes = [];

    public AuroraPipe(DeviceManager deviceManager)
    {
        _deviceManager = deviceManager;

        CreatePipe();
    }

    private void CreatePipe()
    {
        var securityIdentifier = new SecurityIdentifier(WellKnownSidType.WorldSid, null);

        var pipeSecurity = new PipeSecurity();
        pipeSecurity.AddAccessRule(new PipeAccessRule(securityIdentifier,
            PipeAccessRights.ReadWrite | PipeAccessRights.CreateNewInstance | PipeAccessRights.FullControl,
            AccessControlType.Allow));

        var pipe = NamedPipeServerStreamAcl.Create(
            Constants.DeviceManagerPipe, PipeDirection.In,
            NamedPipeServerStream.MaxAllowedServerInstances,
            PipeTransmissionMode.Message, PipeOptions.Asynchronous, 5 * 1024, 5 * 1024, pipeSecurity);
        _pipes.Add(pipe);
        
        Task.Run(() => { pipe.BeginWaitForConnection(ReceiveCommand, pipe); });
    }

    async void ReceiveCommand(IAsyncResult ar)
    {
        Global.Logger.Information("Pipe connection established");

        var pipe = (NamedPipeServerStream)ar.AsyncState!;

        CreatePipe();

        using var sr = new StreamReader(pipe);
        while (pipe.IsConnected && await sr.ReadLineAsync() is { } command)
        {
            using var splits = command.Split(Constants.StringSplit).AsEnumerable().GetEnumerator();

            var word = splits.Next();
            Global.Logger.Information("Received pipe command: {Word}", word);
            switch (word)
            {
                case DeviceCommands.Quit:
                    Global.Logger.Information("Received close command");
                    Shutdown?.Invoke(this, EventArgs.Empty);
                    return;
                case DeviceCommands.Enable:
                {
                    var deviceController = splits.Next();
                    await _deviceManager.Enable(deviceController);
                    break;
                }
                case DeviceCommands.Disable:
                {
                    var deviceController = splits.Next();
                    await _deviceManager.Disable(deviceController);
                    break;
                }
                case DeviceCommands.EnableDevice:
                {
                    var deviceId = splits.Next();
                    await _deviceManager.EnableDevice(deviceId);
                    break;
                }
                case DeviceCommands.DisableDevice:
                {
                    var deviceId = splits.Next();
                    await _deviceManager.DisableDevice(deviceId);
                    break;
                }
                case DeviceCommands.Blink:
                {
                    var deviceId = splits.Next();
                    var deviceLed = (LedId)int.Parse(splits.Next());
                    _deviceManager.BlinkDevice(deviceId, deviceLed);
                    break;
                }
                case DeviceCommands.Remap:
                {
                    var deviceId = splits.Next();
                    var deviceLed = (LedId)int.Parse(splits.Next());
                    var remappedKey = (DeviceKeys)int.Parse(splits.Next());
                    _deviceManager.RemapKey(deviceId, deviceLed, remappedKey);
                    break;
                }
                case DeviceCommands.Unmap:
                {
                    var deviceId = splits.Next();
                    var deviceLed = (LedId)int.Parse(splits.Next());
                    _deviceManager.RemapKey(deviceId, deviceLed, null);
                    break;
                }
                case DeviceCommands.Share:
                {
                    await _deviceManager.ShareRemappableDevices();
                    break;
                }
                case DeviceCommands.Recalibrate:
                {
                    var deviceId = splits.Next();
                    var color = SimpleColor.FromArgb(int.Parse(splits.Next()));

                    Global.DeviceConfig.DeviceCalibrations[deviceId] = color;
                    break;
                }
                default:
                {
                    Global.Logger.Warning("Unknown command: {Command}", command);
                    break;
                }
            }
        }
        Global.Logger.Information("Pipe disconnected");
        CreatePipe();

        await pipe.DisposeAsync();
        _pipes.Remove(pipe);
    }

    public void Dispose()
    {
        foreach (var namedPipe in _pipes)
        {
            namedPipe.Dispose();
        }
        _pipes.Clear();
    }
}