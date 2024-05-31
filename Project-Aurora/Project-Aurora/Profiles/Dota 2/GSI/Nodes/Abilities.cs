using System.Collections;
using System.Collections.Generic;
using System.Linq;
using AuroraRgb.Nodes;

namespace AuroraRgb.Profiles.Dota_2.GSI.Nodes;

/// <summary>
/// Class representing hero abilities
/// </summary>
public class AbilitiesDota2 : Node, IEnumerable<Ability>
{
    private List<Ability> _abilities = [];

    /// <summary>
    /// The attributes a hero has to spend on abilities
    /// </summary>
    public Attributes Attributes { get; }

    private string _json;

    /// <summary>
    /// The number of abilities
    /// </summary>
    public int Count => _abilities.Count;

    internal AbilitiesDota2(string jsonData) : base(jsonData)
    {
        _json = jsonData;

        var abilities = _ParsedData.Properties().Select(p => p.Name).ToList();
        foreach (var abilitySlot in abilities)
        {
            if (abilitySlot.Equals("attributes"))
                Attributes = new Attributes(_ParsedData[abilitySlot].ToString());
            else
                _abilities.Add(new Ability(_ParsedData[abilitySlot].ToString()));
        }
    }

    /// <summary>
    /// Gets the ability at a specified index
    /// </summary>
    /// <param name="index">The index</param>
    /// <returns></returns>
    public Ability this[int index]
    {
        get
        {
            if (index > _abilities.Count - 1)
                return new Ability("");

            return _abilities[index];
        }
    }

    public IEnumerator<Ability> GetEnumerator()
    {
        return _abilities.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return _abilities.GetEnumerator();
    }
}