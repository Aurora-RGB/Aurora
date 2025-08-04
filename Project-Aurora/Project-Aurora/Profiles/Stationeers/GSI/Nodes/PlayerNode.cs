using AuroraRgb.Nodes;

namespace AuroraRgb.Profiles.Stationeers.GSI.Nodes 
{
    public class PlayerNode : Node 
    {

        public int OxygenTankLevel;
        public int OxygenTankCapacity;

        public int WasteTankLevel;
        public int WasteTankCapacity;

        public int Battery;
        public int Food;
        public int Water;
        public int Health;
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
