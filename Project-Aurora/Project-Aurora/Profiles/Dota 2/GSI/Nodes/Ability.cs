using AuroraRgb.Nodes;

namespace AuroraRgb.Profiles.Dota_2.GSI.Nodes;

/// <summary>
/// Class representing ability information
/// </summary>
public class Ability : Node
{
    /// <summary>
    /// Ability name
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// Ability level
    /// </summary>
    public int Level { get; }

    /// <summary>
    /// A boolean representing whether the ability can be casted
    /// </summary>
    public bool CanCast { get; }

    /// <summary>
    /// A boolean representing whether the ability is passive
    /// </summary>
    public bool IsPassive { get; }

    /// <summary>
    /// A boolean representing whether the ability is active
    /// </summary>
    public bool IsActive { get; }

    /// <summary>
    /// Ability cooldown
    /// </summary>
    public int Cooldown { get; }

    /// <summary>
    /// A boolean representing whether the ability is an ultimate
    /// </summary>
    public bool IsUltimate { get; }

    public Ability() : this("")
    {
    }

    internal Ability(string jsonData) : base(jsonData)
    {
        Name = GetString("name");
        Level = GetInt("level");
        CanCast = GetBool("can_cast");
        IsPassive = GetBool("passive");
        IsActive = GetBool("ability_active");
        Cooldown = GetInt("cooldown");
        IsUltimate = GetBool("ultimate");
    }
}