using AuroraRgb.Nodes;

namespace AuroraRgb.Profiles.Stationeers.GSI.Nodes {
    public class WorldNode : Node {

        public float TimeOfDay;

        internal WorldNode(string json) : base(json) 
        {
            TimeOfDay = GetFloat("day_scalar");
        }
    }
}
