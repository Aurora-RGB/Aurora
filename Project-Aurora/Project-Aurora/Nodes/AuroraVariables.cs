using System.Collections.Generic;

namespace AuroraRgb.Nodes;

public class AuroraVariables
{
    public static readonly AuroraVariables Instance = new(); 

    public Dictionary<string, bool> Booleans { get; } = new(8);
    public Dictionary<string, double> Numbers { get; } = new(8);
    public Dictionary<string, string> Strings { get; } = new(8);
}
