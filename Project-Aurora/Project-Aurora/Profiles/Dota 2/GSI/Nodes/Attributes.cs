namespace AuroraRgb.Profiles.Dota_2.GSI.Nodes;

/// <summary>
/// Class representing ability attributes
/// </summary>
public class Attributes
{
    public static readonly Attributes Default = new();
    
    /// <summary>
    /// Amount of levels to spend
    /// </summary>
    public int Level { get; set; }
}