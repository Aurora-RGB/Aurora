
using System.Text.Json.Serialization;
using AuroraRgb.Settings;


[JsonSourceGenerationOptions(WriteIndented = true)]
[JsonSerializable(typeof(SensitiveData))]
public partial class SettingsJsonContext : JsonSerializerContext;