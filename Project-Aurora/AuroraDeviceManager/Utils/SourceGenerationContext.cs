using System.Text.Json.Serialization;
using Common.Devices;
using Common.Devices.RGBNet;

namespace AuroraDeviceManager.Utils;

[JsonSourceGenerationOptions(WriteIndented = false)]
[JsonSerializable(typeof(DeviceConfig))]
[JsonSerializable(typeof(CurrentDevices))]
[JsonSerializable(typeof(DeviceMappingConfig))]
[JsonSerializable(typeof(VariableRegistryItem))]
public partial class SourceGenerationContext : JsonSerializerContext;