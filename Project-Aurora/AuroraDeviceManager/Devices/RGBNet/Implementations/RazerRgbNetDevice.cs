using AuroraDeviceManager.Utils;
using Common.Devices;
using RGB.NET.Core;
using RGB.NET.Devices.Razer;

namespace AuroraDeviceManager.Devices.RGBNet.Implementations;

public class RazerRgbNetDevice : RgbNetDevice
{
    protected override RazerDeviceProvider Provider => RazerDeviceProvider.Instance;

    public override string DeviceName => "Razer (RGB.NET)";

    protected override async Task ConfigureProvider(CancellationToken cancellationToken)
    {
        await base.ConfigureProvider(cancellationToken);
        
        var isRazerServiceRunning = ProcessUtils.IsProcessRunning("rzsdkservice");
        if (!isRazerServiceRunning)
        {
            throw new DeviceProviderException(new ApplicationException("Razer Chroma SDK Service is not running!"), false);
        }

        var loadDevices = RazerEndpointType.None;
        if (Global.DeviceConfig.VarRegistry.GetVariable<bool>($"{DeviceName}_force_all"))
        {
            loadDevices |= RazerEndpointType.All;
        }
        if (Global.DeviceConfig.VarRegistry.GetVariable<bool>($"{DeviceName}_force_keyboard"))
        {
            loadDevices |= RazerEndpointType.Keyboard;
        }
        if (Global.DeviceConfig.VarRegistry.GetVariable<bool>($"{DeviceName}_force_laptop_keyboard"))
        {
            loadDevices |= RazerEndpointType.LaptopKeyboard;
        }
        if (Global.DeviceConfig.VarRegistry.GetVariable<bool>($"{DeviceName}_force_mouse"))
        {
            loadDevices |= RazerEndpointType.Mouse;
        }
        if (Global.DeviceConfig.VarRegistry.GetVariable<bool>($"{DeviceName}_force_headset"))
        {
            loadDevices |= RazerEndpointType.Headset;
        }
        if (Global.DeviceConfig.VarRegistry.GetVariable<bool>($"{DeviceName}_force_mousepad"))
        {
            loadDevices |= RazerEndpointType.Mousepad;
        }
        if (Global.DeviceConfig.VarRegistry.GetVariable<bool>($"{DeviceName}_force_keypad"))
        {
            loadDevices |= RazerEndpointType.Keypad;
        }
        if (Global.DeviceConfig.VarRegistry.GetVariable<bool>($"{DeviceName}_force_link"))
        {
            loadDevices |= RazerEndpointType.ChromaLink;
        }

        
        Provider.LoadEmulatorDevices = loadDevices;
    }

    protected override void RegisterVariables(VariableRegistry variableRegistry)
    {
        base.RegisterVariables(variableRegistry);
        
        variableRegistry.Register($"{DeviceName}_force_all", false, "Force enable all devices");
        variableRegistry.Register($"{DeviceName}_force_keyboard", false, "Force enable keyboard");
        variableRegistry.Register($"{DeviceName}_force_laptop_keyboard", false, "Force enable laptop keyboard");
        variableRegistry.Register($"{DeviceName}_force_mouse", false, "Force enable mouse");
        variableRegistry.Register($"{DeviceName}_force_headset", false, "Force enable headset");
        variableRegistry.Register($"{DeviceName}_force_mousepad", false, "Force enable mousepad");
        variableRegistry.Register($"{DeviceName}_force_keypad", false, "Force enable keypad");
        variableRegistry.Register($"{DeviceName}_force_link", false, "Force enable chroma link");
    }
}