namespace AuroraRgb.Profiles.CSGO.GSI.Nodes;

/// <summary>
/// A class representing the authentication information for GSI
/// </summary>
public class AuthNode
{
    public static readonly AuthNode Default = new();

    /// <summary>
    /// The auth token sent by GSI
    /// </summary>
    public string Token { get; set; } = string.Empty;
}