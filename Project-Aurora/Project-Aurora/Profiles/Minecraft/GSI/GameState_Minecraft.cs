using System.Text.Json.Serialization;
using AuroraRgb.Profiles.Generic;
using AuroraRgb.Profiles.Minecraft.GSI.Nodes;

namespace AuroraRgb.Profiles.Minecraft.GSI;

public partial class GameStateMinecraft : GameState
{
    /// <summary>
    /// Provider node provides information about the data source so that Aurora can update the correct gamestate.
    /// </summary>
    [JsonPropertyName("provider")]
    public ProviderNode Provider { get; set; } = ProviderNode.Default;

    /// <summary>
    /// Player node provides information about the player (e.g. health and hunger).
    /// </summary>
    [JsonPropertyName("game")]
    public MinecraftGameNode Game { get; set; } = MinecraftGameNode.Default;

    /// <summary>
    /// World node provides information about the world (e.g. rain intensity and time).
    /// </summary>
    [JsonPropertyName("world")]
    public MinecraftWorldNode World { get; set; } = MinecraftWorldNode.Default;

    /// <summary>
    /// Player node provides information about the player (e.g. health and hunger).
    /// </summary>
    [JsonPropertyName("player")]
    public MinecraftPlayerNode Player { get; set; } = MinecraftPlayerNode.Default;
}