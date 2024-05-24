using System.Text.Json.Serialization;

namespace AuroraRgb.Modules.Razer.RazerApi;

[JsonSourceGenerationOptions(PropertyNamingPolicy = JsonKnownNamingPolicy.KebabCaseLower)]
[JsonSerializable(typeof(RazerEndpoints))]
public partial class RazerApiSourceGenerationContext : JsonSerializerContext;