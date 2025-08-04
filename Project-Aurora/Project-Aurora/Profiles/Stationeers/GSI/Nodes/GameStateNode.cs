using AuroraRgb.Nodes;
using System.Text.Json.Serialization;

namespace AuroraRgb.Profiles.Stationeers.GSI.Nodes {
    public class GameStateNodeStationeers : Node {
        [JsonPropertyName("game_state")]
        public int GameState;
        /*
        0 = Menu
        1 = Loading
        2 = Playing
        */
        public bool InGame;
        public bool InMenu;
        public bool loading;

        internal GameStateNodeStationeers(string json) : base(json) {

            GameState = GetInt("game_state");
            InGame = GameState == 2;
            InMenu = GameState == 0;
            loading = GameState == 1;
        }
    }
}
