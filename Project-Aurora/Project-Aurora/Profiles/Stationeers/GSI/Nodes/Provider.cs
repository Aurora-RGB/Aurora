using System.Text.Json.Serialization;

namespace AuroraRgb.Profiles.Stationeers.GSI.Nodes;

public class Provider
{
    public static readonly Provider Default = new();
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;
    [JsonPropertyName("appid")]
    public int Appid { get; set; }

    /*    /// <summary>
        /// Game's version
        /// </summary>
        public long Version { get; set; }

        /// <summary>
        /// Local player's Steam ID
        /// </summary>
        [JsonPropertyName("steamid")]
        public string SteamID { get; set; } = string.Empty;

        /// <summary>
        /// Current timestamp
        /// </summary>
        [JsonPropertyName("timestamp")]
        public long TimeStamp { get; set; }*/
}