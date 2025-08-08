
namespace AuroraRgb.Profiles.Stationeers.GSI.Nodes
{
    public class GameStateNode
    {
        public static readonly GameStateNode Default = new();
        public int Gamestate { get; set; }
        /*
        0 = Menu
        1 = Loading
        2 = Playing
        */
        public bool InGame;
        public bool InMenu;
        public bool loading;

        internal GameStateNode()
        {
            InGame = Gamestate == 2;
            InMenu = Gamestate == 0;
            loading = Gamestate == 1;
        }
    }
}
