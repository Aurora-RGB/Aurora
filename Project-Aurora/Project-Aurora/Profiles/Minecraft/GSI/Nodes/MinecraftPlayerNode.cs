using System.Text.Json.Serialization;

namespace AuroraRgb.Profiles.Minecraft.GSI.Nodes;

public class MinecraftPlayerNode
{
    public static readonly MinecraftPlayerNode Default = new();

    [JsonPropertyName("inGame")]
    public bool InGame { get; set; }

    [JsonPropertyName("health")]
    public float Health { get; set; }

    [JsonPropertyName("healthMax")]
    public float HealthMax { get; set; }

    [JsonPropertyName("absorption")]
    public float Absorption { get; set; }

    [JsonPropertyName("absorptionMax")]
    public float AbsorptionMax { get; set; } = 20;

    [JsonPropertyName("isDead")]
    public bool IsDead { get; set; }

    [JsonPropertyName("armor")]
    public int Armor { get; set; }

    [JsonPropertyName("armorMax")]
    public int ArmorMax { get; set; } = 20;

    [JsonPropertyName("experienceLevel")]
    public int ExperienceLevel { get; set; }

    [JsonPropertyName("experience")]
    public float Experience { get; set; }

    [JsonIgnore]
    public float ExperienceMax { get; set; } = 1;

    [JsonPropertyName("foodLevel")]
    public int FoodLevel { get; set; }

    [JsonPropertyName("foodLevelMax")]
    public int FoodLevelMax { get; set; } = 20;

    [JsonPropertyName("saturationLevel")]
    public float SaturationLevel { get; set; }

    [JsonPropertyName("saturationLevelMax")]
    public float SaturationLevelMax { get; set; } = 20;

    [JsonPropertyName("isSneaking")]
    public bool IsSneaking { get; set; }

    [JsonPropertyName("isRidingHorse")]
    public bool IsRidingHorse { get; set; }

    [JsonPropertyName("isBurning")]
    public bool IsBurning { get; set; }

    [JsonPropertyName("isInWater")]
    public bool IsInWater { get; set; }

    [JsonPropertyName("playerEffects")]
    public MinecraftPlayerEffectsNode MinecraftPlayerEffects { get; set; } = MinecraftPlayerEffectsNode.Default;
}