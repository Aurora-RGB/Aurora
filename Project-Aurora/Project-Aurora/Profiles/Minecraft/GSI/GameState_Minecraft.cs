using AuroraRgb.Profiles.Generic;
using AuroraRgb.Profiles.Minecraft.GSI.Nodes;

namespace AuroraRgb.Profiles.Minecraft.GSI;

public class GameStateMinecraft : NewtonsoftGameState {

    /// <summary>
    /// Provider node provides information about the data source so that Aurora can update the correct gamestate.
    /// </summary>
    public ProviderNode Provider => NodeFor<ProviderNode>("provider");

    /// <summary>
    /// Player node provides information about the player (e.g. health and hunger).
    /// </summary>
    public GameNode Game => NodeFor<GameNode>("game");

    /// <summary>
    /// World node provides information about the world (e.g. rain intensity and time).
    /// </summary>
    public WorldNode World => NodeFor<WorldNode>("world");

    /// <summary>
    /// Player node provides information about the player (e.g. health and hunger).
    /// </summary>
    public PlayerNode Player => NodeFor<PlayerNode>("player");

    /// <summary>
    /// Creates a default GameState_Minecraft instance.
    /// </summary>
    public GameStateMinecraft() : base() { }

    /// <summary>
    /// Creates a GameState_Minecraft instance based on the passed JSON data.
    /// </summary>
    /// <param name="jsonString"></param>
    public GameStateMinecraft(string jsonString) : base(jsonString) { }
        
}