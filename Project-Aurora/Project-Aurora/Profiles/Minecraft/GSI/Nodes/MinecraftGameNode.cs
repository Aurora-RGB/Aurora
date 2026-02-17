using System.Text.Json.Serialization;

namespace AuroraRgb.Profiles.Minecraft.GSI.Nodes;

public class MinecraftGameNode
{
    public static readonly MinecraftGameNode Default = new();
    
    [JsonPropertyName("keys")]
    public MinecraftKeyBinding[] KeyBindings { get; set; } = [];

    [JsonPropertyName("controlsGuiOpen")]
    public bool ControlsGuiOpen { get; set; }

    [JsonPropertyName("chatGuiOpen")]
    public bool ChatGuiOpen { get; set; }
}