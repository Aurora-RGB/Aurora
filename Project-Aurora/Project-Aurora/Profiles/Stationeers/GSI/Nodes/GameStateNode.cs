using Newtonsoft.Json;
using System.Text.Json.Serialization;

namespace AuroraRgb.Profiles.Stationeers.GSI.Nodes;

public partial class GameStateNode
{
    public static readonly GameStateNode Default = new();

    // Binds to "game_state" in your JSON payload
    [JsonProperty("game_state")]
    public int GameState { get; set; }

    /*
    0 = Menu
    1 = Loading
    2 = Playing
    */

    // Computed, read-only flags
    public bool InGame => GameState == 2;
    public bool InMenu => GameState == 0;
    public bool Loading => GameState == 1;
}