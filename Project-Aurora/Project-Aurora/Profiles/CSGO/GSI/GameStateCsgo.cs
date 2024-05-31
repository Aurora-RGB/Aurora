using AuroraRgb.Profiles.CSGO.GSI.Nodes;

namespace AuroraRgb.Profiles.CSGO.GSI;

/// <summary>
/// A class representing various information retaining to Game State Integration of Counter-Strike: Global Offensive
/// </summary>
public partial class GameStateCsgo : GameState
{
    private ProviderNode? _provider;
    private MapNode? _map;
    private RoundNode? _round;
    private PlayerNode? _player;
    private AllPlayersNode? _allPlayers;
    private GameStateCsgo? _previously;
    private GameStateCsgo? _added;
    private AuthNode? _auth;

    /// <summary>
    /// Information about the provider of this GameState
    /// </summary>
    public ProviderNode Provider => _provider ??= new ProviderNode(_ParsedData["provider"]?.ToString() ?? "");

    /// <summary>
    /// Information about the current map
    /// </summary>
    public MapNode Map => _map ??= new MapNode(_ParsedData["map"]?.ToString() ?? "");

    /// <summary>
    /// Information about the current round
    /// </summary>
    public RoundNode Round => _round ??= new RoundNode(_ParsedData["round"]?.ToString() ?? "");

    /// <summary>
    /// Information about the current player
    /// </summary>
    public PlayerNode Player => _player ??= new PlayerNode(_ParsedData["player"]?.ToString() ?? "");

    /// <summary>
    /// Information about all players in the lobby
    /// </summary>
    public AllPlayersNode AllPlayers => _allPlayers ??= new AllPlayersNode(_ParsedData["allplayers"]?.ToString() ?? "");

    /// <summary>
    /// A previous GameState
    /// </summary>
    public GameStateCsgo Previously => _previously ??= new GameStateCsgo(_ParsedData["previously"]?.ToString() ?? "");

    /// <summary>
    /// A GameState with only added information
    /// </summary>
    public GameStateCsgo Added => _added ??= new GameStateCsgo(_ParsedData["added"]?.ToString() ?? "");

    /// <summary>
    /// Information about GSI authentication
    /// </summary>
    public AuthNode Auth => _auth ??= new AuthNode(_ParsedData["auth"]?.ToString() ?? "");


    public GameStateCsgo() { }
    public GameStateCsgo(string jsonData) : base(jsonData) { }
}