using AuroraRgb.Nodes;
using System.Text.Json.Serialization;

namespace AuroraRgb.Profiles.Stationeers.GSI.Nodes {
    public class WorldNode : Node {

        [JsonPropertyName("day_scalar")]
        public float TimeOfDay;

        internal WorldNode(string json) : base(json) 
        {
            TimeOfDay = GetFloat("day_scalar");
        }
    }
}
