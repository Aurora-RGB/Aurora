using AuroraRgb.Nodes;

namespace AuroraRgb.Profiles.Dota_2.GSI.Nodes;

/// <summary>
/// Information about the provider of this GameState
/// </summary>
public class Provider_Dota2 : Node
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
    /// Current timestamp
    /// </summary>
    public string TimeStamp { get; }

    internal Provider_Dota2(string jsonData) : base(jsonData)
    {
        Name = GetString("name");
        AppID = GetInt("appid");
        Version = GetInt("version");
        TimeStamp = GetString("timestamp");
    }
}