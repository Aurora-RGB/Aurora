using System.Text.Json.Serialization;

namespace AuroraRgb.Modules.Razer.RazerApi;

[JsonSourceGenerationOptions(PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase)]
[JsonSerializable(typeof(RazerInstallerManifest))]
[JsonSerializable(typeof(RazerManifest))]
public partial class RazerApiSourceGenerationContext : JsonSerializerContext;