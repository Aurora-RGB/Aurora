using Newtonsoft.Json;

namespace AuroraRgb.Profiles.Stationeers.GSI.Nodes;

public class PlayerNode
{
    public static readonly PlayerNode Default = new();

    [JsonProperty("oxygentanklevel")]
    public int OxygenTankLevel { get; set; }
    [JsonProperty("oxygentankcapacity")]
    public int OxygenTankCapacity { get; set; }

    [JsonProperty("wastetanklevel")]
    public int WasteTankLevel { get; set; }
    [JsonProperty("wastetankcapacity")]
    public int WasteTankCapacity { get; set; }

    [JsonProperty("fueltanklevel")]
    public int FuelTankLevel { get; set; }
    [JsonProperty("fueltankcapacity")]
    public int FuelTankCapacity { get; set; }

    public int Battery { get; set; }
    public int Temperature { get; set; } 
    public int Health { get; set; }
    public int Food { get; set; }
    public int Water { get; set; }
    public int Pressure { get; set; }

    [JsonProperty("visorclosed")]
    public bool VisorClosed {  get; set; }
    [JsonProperty("visoropening")]
    public bool VisorOpening { get; set; }
    [JsonProperty("visorclosing")]
    public bool VisorClosing { get; set; }
}