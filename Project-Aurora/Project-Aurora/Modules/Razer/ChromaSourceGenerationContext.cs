using System.Text.Json.Serialization;

namespace AuroraRgb.Modules.Razer;

[JsonSourceGenerationOptions(WriteIndented = true)]
[JsonSerializable(typeof(AuroraChromaSettings))]
public partial class ChromaSourceGenerationContext : JsonSerializerContext;