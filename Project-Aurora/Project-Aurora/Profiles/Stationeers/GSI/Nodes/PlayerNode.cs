using System.Diagnostics;
using System.Text.Json.Serialization;

namespace AuroraRgb.Profiles.Stationeers.GSI.Nodes;

public class PlayerNode
{
    public static readonly PlayerNode Default = new();

    [JsonPropertyName("oxygentanklevel")]
    public int Oxygentanklevel { get; set; }
    [JsonPropertyName("oxygentankcapacity")]
    public int Oxygentankcapacity { get; set; }

    [JsonPropertyName("wastetanklevel")]
    public int Wastetanklevel { get; set; }
    [JsonPropertyName("wastetankcapacity")]
    public int Wastetankcapacity { get; set; }

    [JsonPropertyName("fueltanklevel")]
    public int Fueltanklevel { get; set; }
    [JsonPropertyName("fueltankcapacity")]
    public int Fueltankcapacity { get; set; }

    public int Battery { get; set; }
    public int Temperature { get; set; } 
    public int Health { get; set; }
    public int Food { get; set; }
    public int Water { get; set; }
    public int Pressure { get; set; }
    public bool Visor {  get; set; }
    public bool Visoropening { get; set; }
    public bool Visorclosing { get; set; }
}