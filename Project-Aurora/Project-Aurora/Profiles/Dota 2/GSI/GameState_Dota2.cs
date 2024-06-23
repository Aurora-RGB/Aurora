using AuroraRgb.Profiles.Dota_2.GSI.Nodes;

namespace AuroraRgb.Profiles.Dota_2.GSI;

/// <summary>
/// A class representing various information retaining to Game State Integration of Dota 2
/// </summary>
public partial class GameStateDota2 : GameState
{
    public static readonly GameStateDota2 Default = new();

    /// <summary>
    /// Information about GSI authentication
    /// </summary>
    public AuthDota2 Auth { get; set; } = AuthDota2.Default;

    /// <summary>
    /// Information about the provider of this GameState
    /// </summary>
    public ProviderValve Provider { get; set; } = ProviderValve.Default;

    /// <summary>
    /// Information about the current map
    /// </summary>
    public Map_Dota2 Map { get; set; } = Map_Dota2.Default;

    /// <summary>
    /// Information about the local player
    /// </summary>
    public PlayerDota2 Player { get; set; } = PlayerDota2.Default;

    /// <summary>
    /// Information about the local player's hero
    /// </summary>
    public HeroDota2 Hero { get; set; } = HeroDota2.Default;

    /// <summary>
    /// Information about the local player's hero abilities
    /// </summary>
    [Range(0, 5)]
    public AbilitiesDota2 Abilities { get; set; } = AbilitiesDota2.Default;

    /// <summary>
    /// Information about the local player's hero items
    /// </summary>
    public ItemsDota2 Items { get; set; } = ItemsDota2.Default;

    /// <summary>
    /// A previous GameState
    /// </summary>
    public GameStateDota2? Previously { get; set; }
}