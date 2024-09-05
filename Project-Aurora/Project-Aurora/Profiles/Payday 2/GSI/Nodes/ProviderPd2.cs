using System.Text.Json.Serialization;

namespace AuroraRgb.Profiles.Payday_2.GSI.Nodes;

public class ProviderPd2
{
    public static readonly ProviderPd2 Default = new();

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
    public string Version { get; set; }

    /// <summary>
    /// Local player's Steam ID
    /// </summary>
    [JsonPropertyName("steamid")]
    public string SteamID { get; set; } = string.Empty;

    /// <summary>
    /// Current timestamp
    /// </summary>
    [JsonPropertyName("timestamp")]
    public long TimeStamp { get; set; }
}