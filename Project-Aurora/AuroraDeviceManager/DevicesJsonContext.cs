using System.Text.Json.Serialization;
using Common.Devices;
using Common.Devices.RGBNet;

namespace AuroraDeviceManager;

[JsonSourceGenerationOptions(WriteIndented = true)]
[JsonSerializable(typeof(DeviceConfig))]
[JsonSerializable(typeof(CurrentDevices))]
[JsonSerializable(typeof(DeviceMappingConfig))]
[JsonSerializable(typeof(VariableRegistryItem))]
public partial class DevicesJsonContext : JsonSerializerContext;