using AuroraRgb.Profiles.Generic;
using AuroraRgb.Profiles.Stationeers.GSI.Nodes;

namespace AuroraRgb.Profiles.Stationeers.GSI;

public class GameStateStationeers : NewtonsoftGameState {

    private ProviderNode? _provider;
    private GameStateNodeStationeers? _gameState;
    //private NotificationNode? _notification;
    private WorldNode? _world;
    private PlayerNode? _player;

    /// <summary>
    /// Provider node provides information about the data source so that Aurora can update the correct gamestate.
    /// </summary>
    public ProviderNode Provider => _provider ??= new ProviderNode(ParsedData["provider"]?.ToString() ?? "");

    // Fix ambiguous constructor call by using NodeFor<TNode> for GameStateNode creation
    public GameStateNodeStationeers GameState => _gameState ??= NodeFor<GameStateNodeStationeers>("game_state");

    /// <summary>
    /// Notification node provides information about the Notifications (e.g. Log and Inventory Tab in the PDA).
    /// </summary>
    //public NotificationNode Notification => _notification ??= NodeFor<NotificationNode>("notification");

    /// <summary>
    /// World node provides information about the world (e.g. time).
    /// </summary>
    public WorldNode World => _world ??= NodeFor<WorldNode>("world");

    // Fix ambiguous constructor call by using NodeFor<TNode> for PlayerNode creation
    public PlayerNode Player => _player ??= NodeFor<PlayerNode>("player");

    /// <summary>
    /// Creates a default GameState_Subnautica instance.
    /// </summary>
    public GameStateStationeers()
    { }

    /// <summary>
    /// Creates a GameState_Subnautica instance based on the passed JSON data.
    /// </summary>
    /// <param name="jsonString"></param>
    public GameStateStationeers(string jsonString) : base(jsonString) { }
        
}