using System.Text.Json.Serialization;

namespace AuroraRgb.Modules.Logitech;

[JsonSourceGenerationOptions(WriteIndented = true)]
[JsonSerializable(typeof(AuroraLightsyncSettings))]
public partial class LightsyncSourceGenerationContext : JsonSerializerContext;