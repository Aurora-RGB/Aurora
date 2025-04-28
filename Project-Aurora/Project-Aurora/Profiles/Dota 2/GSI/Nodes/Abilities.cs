using System.Collections.Generic;
using System.Linq;

namespace AuroraRgb.Profiles.Dota_2.GSI.Nodes;

/// <summary>
/// Class representing hero abilities
/// </summary>
public class AbilitiesDota2
{
    public static readonly AbilitiesDota2 Default = new(Attributes.Default, null, null, null, null, null, null);

    private readonly List<Ability> _abilities;

    /// <summary>
    /// The attributes a hero has to spend on abilities
    /// </summary>
    public Attributes Attributes { get; }

    /// <summary>
    /// The number of abilities
    /// </summary>
    public int Count => _abilities.Count;

    public Ability? Ability0 { get; }
    public Ability? Ability1 { get; }
    public Ability? Ability2 { get; }
    public Ability? Ability3 { get; }
    public Ability? Ability4 { get; }
    public Ability? Ability5 { get; }

    [System.Text.Json.Serialization.JsonConstructor]
    public AbilitiesDota2(Attributes attributes, Ability? ability0, Ability? ability1, Ability? ability2, Ability? ability3, Ability? ability4, Ability? ability5)
    {
        Attributes = attributes;
        Ability0 = ability0;
        Ability1 = ability1;
        Ability2 = ability2;
        Ability3 = ability3;
        Ability4 = ability4;
        Ability5 = ability5;

        _abilities = ((Ability?[])[Ability0, Ability1, Ability2, Ability3, Ability4, Ability5])
            .Where(a => a != null).
            Cast<Ability>()
            .ToList();
    }

    /// <summary>
    /// Gets the ability at a specified index
    /// </summary>
    /// <param name="index">The index</param>
    /// <returns></returns>
    public Ability this[int index] => index > _abilities.Count ? Ability.Default : _abilities[index];
}