namespace AuroraRgb.Profiles.Dota_2.GSI.Nodes;

/// <summary>
/// A class representing the authentication information for GSI
/// </summary>
public class AuthDota2
{
    public static readonly AuthDota2 Default = new();
    /// <summary>
    /// The auth token sent by this GSI
    /// </summary>
    public string Token { get; set; } = string.Empty;
}