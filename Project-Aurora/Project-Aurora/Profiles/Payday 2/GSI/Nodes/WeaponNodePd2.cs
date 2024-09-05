using System.Text.Json.Serialization;

namespace AuroraRgb.Profiles.Payday_2.GSI.Nodes;

public class WeaponNodePd2
{
    public static readonly WeaponNodePd2 Default = new();

    [JsonConverter(typeof(JsonStringEnumConverter<WeaponTypePd2>))]
    public WeaponTypePd2 Type { get; set; }

    public int Max { get; set; }

    [JsonPropertyName("current_clip")]
    public int Current_Clip { get; set; }

    [JsonPropertyName("current_left")]
    public int Current_Left { get; set; }

    [JsonPropertyName("max_clip")]
    public int Max_Clip { get; set; }

    [JsonPropertyName("is_selected")]
    public bool IsSelected { get; set; }
}

public enum WeaponTypePd2
{
    Undefined,
    Assault_rifle,
    Pistol,
    Smg,
    LMG,
    Snp,
    Akimbo,
    Shotgun,
    Grenade_launcher,
    Saw,
    Minigun,
    Flamethrower,
    Bow,
    Crossbow
}