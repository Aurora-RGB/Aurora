using AuroraRgb.Profiles.Generic;
using AuroraRgb.Profiles.Subnautica.GSI.Nodes;

namespace AuroraRgb.Profiles.Subnautica.GSI;

public class GameStateSubnautica : NewtonsoftGameState {

    private ProviderNode? _provider;
    private GameStateNode? _gameState;
    private NotificationNode? _notification;
    private WorldNode? _world;
    private PlayerNode? _player;

    /// <summary>
    /// Provider node provides information about the data source so that Aurora can update the correct gamestate.
    /// </summary>
    public ProviderNode Provider => _provider ??= new ProviderNode(ParsedData["provider"]?.ToString() ?? "");

    /// <summary>
    /// Game node provides information about the GameState (InMenu/loading/InGame) source so that Aurora can update the correct gamestate.
    /// </summary>
    public GameStateNode GameState => _gameState ??= new GameStateNode(ParsedData["game_state"]?.ToString() ?? "");

    /// <summary>
    /// Notification node provides information about the Notifications (e.g. Log and Inventory Tab in the PDA).
    /// </summary>
    public NotificationNode Notification => _notification ??= new NotificationNode(ParsedData["notification"]?.ToString() ?? "");

    /// <summary>
    /// World node provides information about the world (e.g. time).
    /// </summary>
    public WorldNode World => _world ??= new WorldNode(ParsedData["world"]?.ToString() ?? "");

    /// <summary>
    /// Player node provides information about the player (e.g. health and hunger).
    /// </summary>
    public PlayerNode Player => _player ??= new PlayerNode(ParsedData["player"]?.ToString() ?? "");

    /// <summary>
    /// Creates a default GameState_Subnautica instance.
    /// </summary>
    public GameStateSubnautica()
    { }

    /// <summary>
    /// Creates a GameState_Subnautica instance based on the passed JSON data.
    /// </summary>
    /// <param name="jsonString"></param>
    public GameStateSubnautica(string jsonString) : base(jsonString) { }
        
}