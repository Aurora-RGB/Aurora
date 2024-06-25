using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;

namespace AuroraRgb.Profiles.Dota_2.GSI.Nodes;

/// <summary>
/// Class representing item information
/// </summary>
public class ItemsDota2
{
    public static readonly ItemsDota2 Default = new(
        Item.Default, Item.Default, Item.Default, Item.Default, Item.Default,
        Item.Default, Item.Default,Item.Default, Item.Default,
        Item.Default, Item.Default, Item.Default, Item.Default, Item.Default, Item.Default,
        Item.Default, Item.Default
        );

    private readonly List<Item> _inventory;
    private readonly List<Item> _stash;

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
    
    
    public Item Slot0 { get; }
    public Item Slot1 { get; }
    public Item Slot2 { get; }
    public Item Slot3 { get; }
    public Item Slot4 { get; }
    public Item Slot5 { get; }
    public Item Slot6 { get; }
    public Item Slot7 { get; }
    public Item Slot8 { get; }
    
    public Item Stash0 { get; }
    public Item Stash1 { get; }
    public Item Stash2 { get; }
    public Item Stash3 { get; }
    public Item Stash4 { get; }
    public Item Stash5 { get; }
    
    public Item Teleport0 { get;}
    public Item Neutral0 { get; }

    [JsonConstructor]
    public ItemsDota2(
        Item slot0, Item slot1, Item slot2, Item slot3, Item slot4, Item slot5, Item slot6, Item slot7, Item slot8,
        Item stash0, Item stash1, Item stash2, Item stash3, Item stash4, Item stash5,
        Item teleport0, Item neutral0)
    {
        Slot0 = slot0;
        Slot1 = slot1;
        Slot2 = slot2;
        Slot3 = slot3;
        Slot4 = slot4;
        Slot5 = slot5;
        Slot6 = slot6;
        Slot7 = slot7;
        Slot8 = slot8;
        Stash0 = stash0;
        Stash1 = stash1;
        Stash2 = stash2;
        Stash3 = stash3;
        Stash4 = stash4;
        Stash5 = stash5;
        Teleport0 = teleport0;
        Neutral0 = neutral0;

        _inventory =
        [
            Slot0, Slot1, Slot2, Slot3, Slot4, Slot5, Slot6, Slot7, Slot8
        ];

        _stash =
        [
            Stash0, Stash1, Stash2, Stash3, Stash4, Stash5
        ];
    }

    /// <summary>
    /// Gets the inventory item at the specified index
    /// </summary>
    /// <param name="index">The index</param>
    /// <returns></returns>
    public Item GetInventoryAt(int index)
    {
        return index > _inventory.Count - 1 ? Item.Default : _inventory[index];
    }

    /// <summary>
    /// Gets the stash item at the specified index
    /// </summary>
    /// <param name="index">The index</param>
    /// <returns></returns>
    public Item GetStashAt(int index)
    {
        return index > _stash.Count - 1 ? Item.Default : _stash[index];
    }

    /// <summary>
    /// Checks if item exists in the inventory
    /// </summary>
    /// <param name="itemName">The item name</param>
    /// <returns>A boolean if item is in the inventory</returns>
    public bool InventoryContains(string itemName)
    {
        return _inventory.Any(inventoryItem => inventoryItem.Name.Equals(itemName));
    }

    /// <summary>
    /// Checks if item exists in the stash
    /// </summary>
    /// <param name="itemName">The item name</param>
    /// <returns>A boolean if item is in the stash</returns>
    public bool StashContains(string itemName)
    {
        return _stash.Any(stashItem => stashItem.Name.Equals(itemName));
    }

    /// <summary>
    /// Gets index of the first occurence of the item in the inventory
    /// </summary>
    /// <param name="itemName">The item name</param>
    /// <returns>The first index at which item is found, -1 if not found.</returns>
    public int InventoryIndexOf(string itemName)
    {
        for (var x = 0; x < _inventory.Count; x++)
        {
            if (_inventory[x].Name.Equals(itemName))
                return x;
        }

        return -1;
    }

    /// <summary>
    /// Gets index of the first occurence of the item in the stash
    /// </summary>
    /// <param name="itemName">The item name</param>
    /// <returns>The first index at which item is found, -1 if not found.</returns>
    public int StashIndexOf(string itemName)
    {
        for (var x = 0; x < _stash.Count; x++)
        {
            if (_stash[x].Name == itemName)
                return x;
        }

        return -1;
    }
}