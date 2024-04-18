using System.Text.Json.Serialization;
using AuroraRgb.Settings.Layouts;

namespace AuroraRgb.Modules.Layouts;

[JsonSerializable(typeof(VirtualGroup))]
[JsonSerializable(typeof(VirtualGroupConfiguration))]
[JsonSerializable(typeof(KeyboardLayout))]
internal partial class LayoutsSourceGenerationContext : JsonSerializerContext;
