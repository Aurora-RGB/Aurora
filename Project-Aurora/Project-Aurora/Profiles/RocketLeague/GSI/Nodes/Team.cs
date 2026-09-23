using System.Drawing;
using System.Text.Json.Serialization;

namespace AuroraRgb.Profiles.RocketLeague.GSI.Nodes;

[method: JsonConstructor]
public class RlTeam(
    string name,
    int teamNum,
    int goals,
    Color colorPrimary,
    Color colorSecondary)
{
    [JsonPropertyName("Name")]
    public string Name { get; } = name;

    [JsonPropertyName("TeamNum")]
    public int TeamNum { get; } = teamNum;

    [JsonPropertyName("Score")]
    public int Goals { get; set; } = goals;

    [JsonPropertyName("ColorPrimary")]
    [JsonConverter(typeof(JsonHexColorConverter))]
    public Color ColorPrimary { get; set; } = colorPrimary;

    [JsonIgnore]
    public double PrimaryAlpha => ColorPrimary.A / 255.0;

    [JsonIgnore]
    public double PrimaryRed => ColorPrimary.R / 255.0;

    [JsonIgnore]
    public double PrimaryGreen => ColorPrimary.G / 255.0;

    [JsonIgnore]
    public double PrimaryBlue => ColorPrimary.B / 255.0;

    [JsonPropertyName("ColorSecondary")]
    [JsonConverter(typeof(JsonHexColorConverter))]
    public Color ColorSecondary { get; } = colorSecondary;

    [JsonIgnore]
    public double SecondaryAlpha => ColorPrimary.A / 255.0;

    [JsonIgnore]
    public double SecondaryRed => ColorSecondary.R / 255.0;

    [JsonIgnore]
    public double SecondaryGreen => ColorSecondary.G / 255.0;

    [JsonIgnore]
    public double SecondaryBlue => ColorSecondary.B / 255.0;
}

[method: JsonConstructor]
public class RlAttacker(
    string name,
    int shortcut,
    int teamNum)
{
    [JsonPropertyName("Name")]
    public string Name { get; } = name;

    [JsonPropertyName("Shortcut")]
    public int Shortcut { get; } = shortcut;

    [JsonPropertyName("TeamNum")]
    public int TeamNum { get; } = teamNum;
}

[method: JsonConstructor]
public class RlTarget(
    string name,
    int shortcut,
    int teamNum)
{
    [JsonPropertyName("Name")]
    public string Name { get; } = name;

    [JsonPropertyName("Shortcut")]
    public int Shortcut { get; } = shortcut;

    [JsonPropertyName("TeamNum")]
    public int TeamNum { get; } = teamNum;
}