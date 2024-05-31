using AuroraRgb.Profiles.Dota_2.GSI.Nodes;

namespace AuroraRgb.Profiles.Dota_2.GSI;

/// <summary>
/// A class representing various information retaining to Game State Integration of Dota 2
/// </summary>
public partial class GameStateDota2 : GameState
{
    private Auth_Dota2? _auth;
    private Provider_Dota2? _provider;
    private Map_Dota2? _map;
    private PlayerDota2? _player;
    private HeroDota2? _hero;
    private AbilitiesDota2? _abilities;
    private Items_Dota2? _items;
    private GameStateDota2? _previously;
    private GameStateDota2? _added;

    public GameStateDota2()
    { }
    public GameStateDota2(string jsonData) : base(jsonData) { }

    /// <summary>
    /// Information about GSI authentication
    /// </summary>
    public Auth_Dota2 Auth => _auth ??= new Auth_Dota2(GetNode("auth"));

    /// <summary>
    /// Information about the provider of this GameState
    /// </summary>
    public Provider_Dota2 Provider => _provider ??= new Provider_Dota2(GetNode("provider"));

    /// <summary>
    /// Information about the current map
    /// </summary>
    public Map_Dota2 Map => _map ??= new Map_Dota2(GetNode("map"));

    /// <summary>
    /// Information about the local player
    /// </summary>
    public PlayerDota2 Player => _player ??= new PlayerDota2(GetNode("player"));

    /// <summary>
    /// Information about the local player's hero
    /// </summary>
    public HeroDota2 Hero => _hero ??= new HeroDota2(GetNode("hero"));

    /// <summary>
    /// Information about the local player's hero abilities
    /// </summary>
    [Range(0, 5)]
    public AbilitiesDota2 Abilities => _abilities ??= new AbilitiesDota2(GetNode("abilities"));

    /// <summary>
    /// Information about the local player's hero items
    /// </summary>
    public Items_Dota2 Items => _items ??= new Items_Dota2(GetNode("items"));

    /// <summary>
    /// A previous GameState
    /// </summary>
    public GameStateDota2 Previously => _previously ??= new GameStateDota2(GetNode("previously"));
}