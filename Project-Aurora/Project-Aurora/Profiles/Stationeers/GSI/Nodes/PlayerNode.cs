using AuroraRgb.Nodes;
using System.Text.Json.Serialization;

namespace AuroraRgb.Profiles.Stationeers.GSI.Nodes 
{
    public class PlayerNode : Node 
    {
        [JsonPropertyName("oxygentanklevel")]
        public int OxygenTankLevel;
        [JsonPropertyName("oxygentankcapacity")]
        public int OxygenTankCapacity;

        [JsonPropertyName("wastetanklevel")]
        public int WasteTankLevel;
        [JsonPropertyName("wastetankcapacity")]
        public int WasteTankCapacity;

        [JsonPropertyName("battery")]
        public int Battery;
        [JsonPropertyName("food")]
        public int Food;
        [JsonPropertyName("water")]
        public int Water;
        [JsonPropertyName("health")]
        public int Health;
        [JsonPropertyName("temp")]
        public int Temperature;

        internal PlayerNode(string json) : base(json) 
        {
            
            OxygenTankLevel = GetInt("oxygentanklevel");
            OxygenTankCapacity = GetInt("oxygentankcapacity");

            WasteTankLevel = GetInt("wastetanklevel");
            WasteTankCapacity = GetInt("wastetankcapacity");

            Battery = GetInt("battery");
            Food = GetInt("food");
            Water = GetInt("water");
            Health = GetInt("health");
            Temperature = GetInt("temp");
        }
    }
}
