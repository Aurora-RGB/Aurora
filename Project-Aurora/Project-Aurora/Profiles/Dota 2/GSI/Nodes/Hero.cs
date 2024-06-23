using System.Text.Json.Serialization;

namespace AuroraRgb.Profiles.Dota_2.GSI.Nodes;

/// <summary>
/// Class representing hero information
/// </summary>
public class HeroDota2
{
    public static readonly HeroDota2 Default = new();

    /// <summary>
    /// Hero ID
    /// </summary>
    [JsonPropertyName("id")]
    public int ID { get; set; }

    /// <summary>
    /// Hero name
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// Hero level
    /// </summary>
    public int Level { get; set; }

    /// <summary>
    /// A boolean representing whether the hero is alive
    /// </summary>
    [JsonPropertyName("alive")]
    public bool IsAlive { get; set; }

    /// <summary>
    /// Amount of seconds until the hero respawns
    /// </summary>
    [JsonPropertyName("respawn_seconds")]
    public int SecondsToRespawn { get; set; }

    /// <summary>
    /// The buyback cost
    /// </summary>
    public int BuybackCost { get; set; }

    /// <summary>
    /// The buyback cooldown
    /// </summary>
    public int BuybackCooldown { get; set; }

    /// <summary>
    /// Hero health
    /// </summary>
    public int Health { get; set; }

    /// <summary>
    /// Hero max health
    /// </summary>
    public int MaxHealth { get; set; }

    /// <summary>
    /// Hero health percentage
    /// </summary>
    public int HealthPercent { get; set; }

    /// <summary>
    /// Hero mana
    /// </summary>
    public int Mana { get; set; }

    /// <summary>
    /// Hero max mana
    /// </summary>
    public int MaxMana { get; set; }

    /// <summary>
    /// Hero mana percent
    /// </summary>
    public int ManaPercent { get; set; }

    /// <summary>
    /// A boolean representing whether the hero is silenced
    /// </summary>
    [JsonPropertyName("silenced")]
    public bool IsSilenced { get; set; }

    /// <summary>
    /// A boolean representing whether the hero is stunned
    /// </summary>
    [JsonPropertyName("stunned")]
    public bool IsStunned { get; set; }

    /// <summary>
    /// A boolean representing whether the hero is disarmed
    /// </summary>
    [JsonPropertyName("disarmed")]
    public bool IsDisarmed { get; set; }

    /// <summary>
    /// A boolean representing whether the hero is magic immune
    /// </summary>
    [JsonPropertyName("magicimmune")]
    public bool IsMagicImmune { get; set; }

    /// <summary>
    /// A boolean representing whether the hero is hexed
    /// </summary>
    [JsonPropertyName("hexed")]
    public bool IsHexed { get; set; }

    /// <summary>
    /// A boolean representing whether the hero is muteds
    /// </summary>
    [JsonPropertyName("muted")]
    public bool IsMuted { get; set; }

    /// <summary>
    /// A boolean representing whether the hero is broken
    /// </summary>
    [JsonPropertyName("break")]
    public bool IsBreak { get; set; }

    /// <summary>
    /// A boolean representing whether the hero has Aghanim's Scepter
    /// </summary>
    [JsonPropertyName("aghanims_scepter")]
    public bool HasScepter { get; set; }

    /// <summary>
    /// A boolean representing whether the hero has Aghanim's Shard
    /// </summary>
    [JsonPropertyName("aghanims_shard")]
    public bool HasShard { get; set; }

    /// <summary>
    /// A boolean representing whether the hero is under smoke effect
    /// </summary>
    [JsonPropertyName("smoked")]
    public bool IsSmoked { get; set; }

    /// <summary>
    /// A boolean representing whether the hero is debuffed
    /// </summary>
    public bool HasDebuff { get; set; }

    [JsonPropertyName("talent_1")]
    public bool HasTalent1 { get; set; }
    [JsonPropertyName("talent_2")]
    public bool HasTalent2 { get; set; }
    [JsonPropertyName("talent_3")]
    public bool HasTalent3 { get; set; }
    [JsonPropertyName("talent_4")]
    public bool HasTalent4 { get; set; }
    [JsonPropertyName("talent_5")]
    public bool HasTalent5 { get; set; }
    [JsonPropertyName("talent_6")]
    public bool HasTalent6 { get; set; }
    [JsonPropertyName("talent_7")]
    public bool HasTalent7 { get; set; }
    [JsonPropertyName("talent_8")]
    public bool HasTalent8 { get; set; }
}