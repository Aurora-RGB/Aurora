using AuroraRgb.Nodes;

namespace AuroraRgb.Profiles.CSGO.GSI.Nodes;

/// <summary>
/// Information about the provider of this GameState
/// </summary>
public class ProviderNode : Node
{
    /// <summary>
    /// Game name
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// Game's Steam AppID
    /// </summary>
    public int AppID { get; }

    /// <summary>
    /// Game's version
    /// </summary>
    public int Version { get; }

    /// <summary>
    /// Local player's Steam ID
    /// </summary>
    public string SteamID { get; set; }

    /// <summary>
    /// Current timestamp
    /// </summary>
    public string TimeStamp { get; }

    internal ProviderNode(string json)
        : base(json)
    {
        Name = GetString("name");
        AppID = GetInt("appid");
        Version = GetInt("version");
        SteamID = GetString("steamid");
        TimeStamp = GetString("timestamp");
    }
}