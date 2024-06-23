using AuroraRgb.Profiles.Generic;
using AuroraRgb.Profiles.Slime_Rancher.GSI.Nodes;

namespace AuroraRgb.Profiles.Slime_Rancher.GSI;

public class GameStateSlimeRancher : NewtonsoftGameState {

    public ProviderNode Provider => NodeFor<ProviderNode>("provider");
    public GameStateNode GameState => NodeFor<GameStateNode>("game_state");
    public PlayerNode Player => NodeFor<PlayerNode>("player");
    public VacPackNode VacPack => NodeFor<VacPackNode>("vac_pack");
    public MailNode Mail => NodeFor<MailNode>("mail");
    public WorldNode World => NodeFor<WorldNode>("world");
    public LocationNode Location => NodeFor<LocationNode>("location");

    public GameStateSlimeRancher()
    { }

    /// <summary>
    /// Creates a GameState_Slime_Rancher instance based on the passed JSON data.
    /// </summary>
    /// <param name="jsonString"></param>
    public GameStateSlimeRancher(string jsonString) : base(jsonString) { }
}