using System.Text.Json.Serialization;

namespace AuroraRgb.Modules.Gamebar;

[JsonSourceGenerationOptions(WriteIndented = true)]
[JsonSerializable(typeof(GamebarConfig))]
public partial class GamebarSourceGenerationContext : JsonSerializerContext;