using System.Text.Json.Serialization;

namespace AuroraRgb.Profiles.Dota_2.GSI.Nodes;

/// <summary>
/// Information about the provider of this GameState
/// </summary>
public class ProviderValve
{
    public static readonly ProviderValve Default = new();

    /// <summary>
    /// Game name
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Game's Steam AppID
    /// </summary>
    [JsonPropertyName("appid")]
    public int AppID { get; set; }

    /// <summary>
    /// Game's version
    /// </summary>
    public int Version { get; set; }

    /// <summary>
    /// Local player's Steam ID
    /// </summary>
    [JsonPropertyName("steamid")]
    public string SteamID { get; set; } = string.Empty;

    /// <summary>
    /// Current timestamp
    /// </summary>
    [JsonPropertyName("timestamp")]
    public int TimeStamp { get; set; }
}