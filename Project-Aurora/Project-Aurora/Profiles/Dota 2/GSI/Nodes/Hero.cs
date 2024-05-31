using AuroraRgb.Nodes;

namespace AuroraRgb.Profiles.Dota_2.GSI.Nodes;

/// <summary>
/// Class representing hero information
/// </summary>
public class HeroDota2 : Node
{
    /// <summary>
    /// Hero ID
    /// </summary>
    public int ID { get; }

    /// <summary>
    /// Hero name
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// Hero level
    /// </summary>
    public int Level { get; }

    /// <summary>
    /// A boolean representing whether the hero is alive
    /// </summary>
    public bool IsAlive { get; set; }

    /// <summary>
    /// Amount of seconds until the hero respawns
    /// </summary>
    public int SecondsToRespawn { get; set; }

    /// <summary>
    /// The buyback cost
    /// </summary>
    public int BuybackCost { get; }

    /// <summary>
    /// The buyback cooldown
    /// </summary>
    public int BuybackCooldown { get; }

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
    public bool IsSilenced { get; }

    /// <summary>
    /// A boolean representing whether the hero is stunned
    /// </summary>
    public bool IsStunned { get; }

    /// <summary>
    /// A boolean representing whether the hero is disarmed
    /// </summary>
    public bool IsDisarmed { get; }

    /// <summary>
    /// A boolean representing whether the hero is magic immune
    /// </summary>
    public bool IsMagicImmune { get; }

    /// <summary>
    /// A boolean representing whether the hero is hexed
    /// </summary>
    public bool IsHexed { get; }

    /// <summary>
    /// A boolean representing whether the hero is muteds
    /// </summary>
    public bool IsMuted { get; }

    /// <summary>
    /// A boolean representing whether the hero is broken
    /// </summary>
    public bool IsBreak { get; }

    /// <summary>
    /// A boolean representing whether the hero has Aghanim's Scepter
    /// </summary>
    public bool HasScepter { get; }

    /// <summary>
    /// A boolean representing whether the hero has Aghanim's Shard
    /// </summary>
    public bool HasShard { get; }

    /// <summary>
    /// A boolean representing whether the hero is under smoke effect
    /// </summary>
    public bool IsSmoked { get; }

    /// <summary>
    /// A boolean representing whether the hero is debuffed
    /// </summary>
    public bool HasDebuff { get; }

    public bool HasTalent1 { get; }
    public bool HasTalent2 { get; }
    public bool HasTalent3 { get; }
    public bool HasTalent4 { get; }
    public bool HasTalent5 { get; }
    public bool HasTalent6 { get; }
    public bool HasTalent7 { get; }
    public bool HasTalent8 { get; }

    internal HeroDota2(string jsonData) : base(jsonData)
    {
        ID = GetInt("id");
        Name = GetString("name");
        Level = GetInt("level");
        IsAlive = GetBool("alive");
        SecondsToRespawn = GetInt("respawn_seconds");
        BuybackCost = GetInt("buyback_cost");
        BuybackCooldown = GetInt("buyback_cooldown");
        Health = GetInt("health");
        MaxHealth = GetInt("max_health");
        HealthPercent = GetInt("health_percent");
        Mana = GetInt("mana");
        MaxMana = GetInt("max_mana");
        ManaPercent = GetInt("mana_percent");
        IsSilenced = GetBool("silenced");
        IsStunned = GetBool("stunned");
        IsDisarmed = GetBool("disarmed");
        IsMagicImmune = GetBool("magicimmune");
        IsHexed = GetBool("hexed");
        IsMuted = GetBool("muted");
        IsBreak = GetBool("break");
        HasScepter = GetBool("aghanims_scepter");
        HasShard = GetBool("aghanims_shard");
        IsSmoked = GetBool("smoked");
        HasDebuff = GetBool("has_debuff");
        HasTalent1 = GetBool("talen1_1");
        HasTalent2 = GetBool("talen1_2");
        HasTalent3 = GetBool("talen1_3");
        HasTalent4 = GetBool("talen1_4");
        HasTalent5 = GetBool("talen1_5");
        HasTalent6 = GetBool("talen1_6");
        HasTalent7 = GetBool("talen1_7");
        HasTalent8 = GetBool("talen1_8");
    }
}