using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Text.Json.Serialization;

namespace Common.Devices;

public sealed partial class DeviceConfig : INotifyPropertyChanged, IAuroraConfig
{
    public const string FileName = "DeviceConfig.json";

    [JsonIgnore]
    public static readonly string ConfigFile =
        Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Aurora", FileName);

    public string ConfigPath => ConfigFile;

    public event PropertyChangedEventHandler? PropertyChanged;

    public Dictionary<string, SimpleColor> DeviceCalibrations { get; set; } = new();

    [JsonPropertyName("allow_peripheral_devices")]
    public bool AllowPeripheralDevices { get; set; } = true;

    [JsonPropertyName("devices_disable_keyboard")]
    public bool DevicesDisableKeyboard { get; set; }

    [JsonPropertyName("devices_disable_mouse")]
    public bool DevicesDisableMouse { get; set; }

    [JsonPropertyName("devices_disable_headset")]
    public bool DevicesDisableHeadset { get; set; }
    
    public bool DangerousOpenRgbNonDirectEnable { get; set; }

    private ObservableCollection<string>? _enabledControllers;
    [JsonPropertyName("EnabledDevices")]
    public ObservableCollection<string> EnabledControllers
    {
        get => _enabledControllers ??= new ObservableCollection<string>(DefaultEnabledControllers);
        set => _enabledControllers = value;
    }

    private ObservableCollection<string>? _disabledControllerDevices;
    public ObservableCollection<string> DisabledControllerDevices
    {
        get => _disabledControllerDevices ??= [];
        set => _disabledControllerDevices = value;
    }

    private static readonly Dictionary<string,string> Migrations = new()
    {
        {"Aurora.Devices.AtmoOrbDevice.AtmoOrbDevice", "AtmoOrb"},
        {"Aurora.Devices.Bloody.BloodyDevice" , "Bloody"},
        {"Aurora.Devices.Clevo.ClevoDevice" , "Clevo Keyboard"},
        {"Aurora.Devices.Creative.SoundBlasterXDevice", "SoundBlasterX"},
        {"Aurora.Devices.Drevo.BloodyDevice" , "DrevoDevice"},
        {"Aurora.Devices.Dualshock4.DualshockDevice" , "Sony DualShock 4(PS4)"},
        {"Aurora.Devices.Ducky.DuckyDevice", "Ducky"},
        {"Aurora.Devices.LightFX.LightFxDevice" , "LightFX"},
        {"Aurora.Devices.Omen.OmenDevices" , "OMEN"},
        {"Aurora.Devices.Razer.RazerDevice", "Razer (RGB.NET)"},
        {"Aurora.Devices.Roccat.RoccatDevice" , "Roccat"},
        {"Aurora.Devices.SteelSeries.SteelSeriesDevice" , "SteelSeries"},
        {"Aurora.Devices.UnifiedHID.UnifiedHIDDevice" , "UnifiedHID"},
        {"Aurora.Devices.Vulcan.VulcanDevice" , "Vulcan"},
        {"Aurora.Devices.Wooting.WootingDevice", "Wooting (RGB.NET)"},
        {"Aurora.Devices.YeeLight.YeeLightDevice" , "YeeLight"},
        {"Aurora.Devices.OpenRGB", "OpenRGB (RGB.NET)"},

        {"Aurora.Devices.RGBNet.AsusDevice", "Asus (RGB.NET)"},
        {"Aurora.Devices.RGBNet.BloodyRgbNetDevice", "Bloody (RGB.NET)"},
        {"Aurora.Devices.RGBNet.CoolerMasterRgbNetDevice", "CoolerMaster (RGB.NET)"},
        {"Aurora.Devices.RGBNet.CorsairRgbNetDevice", "Corsair (RGB.NET)"},
        {"Aurora.Devices.RGBNet.LogitechRgbNetDevice", "Logitech (RGB.NET)"},
        {"Aurora.Devices.RGBNet.OpenRgbNetDevice", "OpenRGB (RGB.NET)"},
        {"Aurora.Devices.RGBNet.RazerRgbNetDevice", "Razer (RGB.NET)"},
        {"Aurora.Devices.RGBNet.SteelSeriesRgbNetDevice", "SteelSeries (RGB.NET)"},
        {"Aurora.Devices.RGBNet.WootingRgbNetDevice", "Wooting (RGB.NET)"},
        {"Aurora.Devices.RGBNet.YeelightRgbNetDevice", "Yeelight (RGB.NET)"},
        
        {"Razer", "Razer (RGB.NET)"},
        {"Wooting", "Wooting (RGB.NET)"},
    };

    private static HashSet<string> DefaultEnabledControllers =>
    [
        "Corsair (RGB.NET)",
        "Logitech (RGB.NET)",
        "OpenRGB (RGB.NET)"
    ];

    public VariableRegistry VarRegistry { get; set; } = new();

    public void OnPostLoad()
    {
        _enabledControllers ??= new ObservableCollection<string>(DefaultEnabledControllers);

        MigrateDevices();

        PrioritizeDevice("Logitech (RGB.NET)", "Logitech");

        EnabledControllers.CollectionChanged += (_, _) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(EnabledControllers)));
    }

    private void MigrateDevices()
    {
        foreach (var loadedDeviceString in EnabledControllers.ToArray())
        {
            var typeName = loadedDeviceString.Split(",")[0];
            if (!Migrations.TryGetValue(typeName, out var migratedValue)) continue;
            EnabledControllers.Remove(loadedDeviceString);
            EnabledControllers.Add(migratedValue);
        }
    }

    private void PrioritizeDevice(string primary, string secondary)
    {
        if (EnabledControllers.Contains(primary))
        {
            EnabledControllers.Remove(secondary);
        }
    }
}