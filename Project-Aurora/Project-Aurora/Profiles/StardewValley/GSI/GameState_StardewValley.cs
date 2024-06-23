using AuroraRgb.Profiles.Generic;
using AuroraRgb.Profiles.StardewValley.GSI.Nodes;

namespace AuroraRgb.Profiles.StardewValley.GSI;

public class GameStateStardewValley : NewtonsoftGameState {
    public ProviderNode Provider => NodeFor<ProviderNode>("Provider");

    public WorldNode World => NodeFor<WorldNode>("World");
    public PlayerNode Player => NodeFor<PlayerNode>("Player");
    public InventoryNode Inventory => NodeFor<InventoryNode>("Inventory");

    public JournalNode Journal => NodeFor<JournalNode>("Journal");
    public GameStatusNode Game => NodeFor<GameStatusNode>("Game");

    public GameStateStardewValley()
    { }

    /// <summary>
    /// Creates a GameState_StardewValley instance based on the passed JSON data.
    /// </summary>
    /// <param name="jsonString"></param>
    public GameStateStardewValley(string jsonString) : base(jsonString) { }
}