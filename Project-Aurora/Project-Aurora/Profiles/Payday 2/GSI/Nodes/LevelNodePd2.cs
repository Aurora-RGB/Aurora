using System.ComponentModel;
using System.Text.Json.Serialization;

namespace AuroraRgb.Profiles.Payday_2.GSI.Nodes;

/// <summary>
/// Information about the level
/// </summary>
public class LevelNodePd2
{
    public static readonly LevelNodePd2  Default = new();

    /// <summary>
    /// Level ID
    /// </summary>
    [JsonPropertyName("level_id")]
    public string LevelID { get; set; } = string.Empty;

    /// <summary>
    /// Level phase
    /// </summary>
    [JsonConverter(typeof(JsonStringEnumConverter<LevelPhase>))]
    public LevelPhase Phase { get; set; }

    /// <summary>
    /// Counter for point of no return
    /// </summary>
    [JsonPropertyName("no_return_timer")]
    public int NoReturnTime { get; set; }
}

/// <summary>
/// Enum for each level phase
/// </summary>
public enum LevelPhase
{
    /// <summary>
    /// Undefined
    /// </summary>
    [Description("Undefined")]
    Undefined,

    /// <summary>
    /// Stealth
    /// </summary>
    [Description("Stealth")]
    Stealth,

    /// <summary>
    /// Loud
    /// </summary>
    [Description("Loud")]
    Loud,

    /// <summary>
    /// Captain Winterss
    /// </summary>
    [Description("Cptn. Winters")]
    Winters,

    /// <summary>
    /// Assault
    /// </summary>
    [Description("Assault")]
    Assault,

    /// <summary>
    /// Point of no Return
    /// </summary>
    [Description("Point of no return")]
    Point_of_no_return
}