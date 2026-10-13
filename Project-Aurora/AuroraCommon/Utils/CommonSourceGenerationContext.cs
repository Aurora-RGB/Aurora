using System.Text.Json.Serialization;
using Common.Devices;
using Common.Devices.RGBNet;

namespace Common.Utils;

[JsonSourceGenerationOptions(WriteIndented = true)]
[JsonSerializable(typeof(DeviceMappingConfig))]
[JsonSerializable(typeof(DeviceConfig))]
[JsonSerializable(typeof(DeviceCalibration))]
[JsonSerializable(typeof(CalibrationCurve))]
[JsonSerializable(typeof(ColorMatrix))]
[JsonSerializable(typeof(ColorSample))]
public partial class CommonSourceGenerationContext : JsonSerializerContext;