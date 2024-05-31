using System.Collections.Generic;
using System.Linq;
using AuroraRgb.Nodes;

namespace AuroraRgb.Profiles.Dota_2.GSI.Nodes;

/// <summary>
/// Class representing item information
/// </summary>
public class Items_Dota2 : Node
{
    private readonly List<Item> _inventory = [];
    private readonly List<Item> _stash = [];

    /// <summary>
    /// Number of items in the inventory
    /// </summary>
    public int InventoryCount => _inventory.Count;

    /// <summary>
    /// Gets the array of the inventory items
    /// </summary>
    [Range(0, 8)]
    public Item[] InventoryItems => _inventory.ToArray();

    /// <summary>
    /// Number of items in the stash
    /// </summary>
    public int StashCount => _stash.Count;

    /// <summary>
    /// Gets the array of the stash items
    /// </summary>
    [Range(0, 5)]
    public Item[] StashItems => _stash.ToArray();

    internal Items_Dota2(string jsonData) : base(jsonData)
    {
        var slots = _ParsedData.Properties().Select(p => p.Name).ToList();
            
        foreach (var itemSlot in slots)
        {
            if (itemSlot.StartsWith("slot"))
                _inventory.Add(new Item(_ParsedData[itemSlot].ToString()));
            else if(itemSlot.StartsWith("stash"))
                _stash.Add(new Item(_ParsedData[itemSlot].ToString()));
        }
    }

    /// <summary>
    /// Gets the inventory item at the specified index
    /// </summary>
    /// <param name="index">The index</param>
    /// <returns></returns>
    public Item GetInventoryAt(int index)
    {
        if (index > _inventory.Count - 1)
            return new Item("");

        return _inventory[index];
    }

    /// <summary>
    /// Gets the stash item at the specified index
    /// </summary>
    /// <param name="index">The index</param>
    /// <returns></returns>
    public Item GetStashAt(int index)
    {
        if (index > _stash.Count - 1)
            return new Item("");

        return _stash[index];
    }

    /// <summary>
    /// Checks if item exists in the inventory
    /// </summary>
    /// <param name="itemName">The item name</param>
    /// <returns>A boolean if item is in the inventory</returns>
    public bool InventoryContains(string ItemName)
    {
        foreach(var InventoryItem in _inventory)
        {
            if (InventoryItem.Name.Equals(ItemName))
                return true;
        }

        return false;
    }

    /// <summary>
    /// Checks if item exists in the stash
    /// </summary>
    /// <param name="itemname">The item name</param>
    /// <returns>A boolean if item is in the stash</returns>
    public bool StashContains(string ItemName)
    {
        foreach (var StashItem in _stash)
        {
            if (StashItem.Name.Equals(ItemName))
                return true;
        }

        return false;
    }

    /// <summary>
    /// Gets index of the first occurence of the item in the inventory
    /// </summary>
    /// <param name="itemName">The item name</param>
    /// <returns>The first index at which item is found, -1 if not found.</returns>
    public int InventoryIndexOf(string ItemName)
    {
        for (var x = 0; x < _inventory.Count; x++)
        {
            if (_inventory[x].Name.Equals(ItemName))
                return x;
        }

        return -1;
    }

    /// <summary>
    /// Gets index of the first occurence of the item in the stash
    /// </summary>
    /// <param name="itemname">The item name</param>
    /// <returns>The first index at which item is found, -1 if not found.</returns>
    public int StashIndexOf(string ItemName)
    {
        for (var x = 0; x < _stash.Count; x++)
        {
            if (_stash[x].Name == ItemName)
                return x;
        }

        return -1;
    }
}