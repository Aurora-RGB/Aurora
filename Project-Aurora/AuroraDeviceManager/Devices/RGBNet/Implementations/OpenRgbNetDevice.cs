using System.Net;
using AuroraDeviceManager.Utils;
using Common.Devices;
using RGB.NET.Core;
using RGB.NET.Devices.OpenRGB;

namespace AuroraDeviceManager.Devices.RGBNet.Implementations;

public class OpenRgbNetDevice : RgbNetDevice
{ 
    public override string DeviceName => "OpenRGB (RGB.NET)";

    protected override OpenRGBDeviceProvider Provider => OpenRGBDeviceProvider.Instance;
    
    private readonly OpenRGBServerDefinition _openRgbServerDefinition = new()
    {
        ClientName = "Aurora (RGB.NET)"
    };

    protected override Task ConfigureProvider(CancellationToken cancellationToken)
    {
        base.ConfigureProvider(cancellationToken);
        
        var ip = Global.DeviceConfig.VarRegistry.GetString($"{DeviceName}_ip");
        var port = Global.DeviceConfig.VarRegistry.GetVariable<int>($"{DeviceName}_port");

        if (ip == IPAddress.Loopback.ToString())
        {
            var isOpenRgbRunning = ProcessUtils.IsProcessRunning("OpenRGB");
            if (!isOpenRgbRunning)
            {
                //TODO check sdk enabled
                throw new DeviceProviderException(new ApplicationException("OpenRGB is not running!"), false);
            }
        }

        if (Global.DeviceConfig.DangerousOpenRgbNonDirectEnable)
        {
            Provider.ForceAddAllDevices = true;
        }

        _openRgbServerDefinition.Ip = ip;
        _openRgbServerDefinition.Port = port;

        Provider.AddDeviceDefinition(_openRgbServerDefinition);
        return Task.CompletedTask;
    }

    protected override void RegisterVariables(VariableRegistry variableRegistry)
    {
        base.RegisterVariables(variableRegistry);
        
        variableRegistry.Register($"{DeviceName}_sleep", 0, "Sleep for", 1000, 0);
        variableRegistry.Register($"{DeviceName}_ip", "127.0.0.1", "IP Address");
        variableRegistry.Register($"{DeviceName}_port", 6742, "Port", 65535, 1024);
        variableRegistry.Register($"{DeviceName}_fallback_key", DeviceKeys.Peripheral_Logo, "Key to use for unknown leds. Select NONE to disable");
    }

    public override bool HotplugEnabled()
    {
        return true;
    }
}