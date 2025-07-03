// ItemData.cs
using System;
using System.Collections.Generic;
using Unity.Collections;

[Serializable]
public struct ItemData
{
    /// <summary>
    /// Unique ID used for network syncing and database lookups.
    /// </summary>
    public FixedString32Bytes itemId;

    /// <summary>
    /// Display name.
    /// </summary>
    public string itemName;

    /// <summary>
    /// Reference to the icon asset (by ID, not Sprite).
    /// </summary>
    public FixedString32Bytes iconId;

    /// <summary>
    /// Max stack size for this item type.
    /// </summary>
    public int stackSize;

    /// <summary>
    /// Arbitrary stats, e.g. durability, weight, etc.
    /// </summary>
    public Dictionary<string, float> stats;

    /// <summary>
    /// Tags for categorization (e.g. "Food", "Tool").
    /// </summary>
    public List<string> tags;

    public static ItemData Empty()
    {
        return new ItemData
        {
            itemId = string.Empty,
            itemName = string.Empty,
            iconId = string.Empty,
            stackSize = 0,
            stats = new Dictionary<string, float>(),
            tags = new List<string>()
        };
    }

    public bool IsEmpty()
    {
        return itemId.IsEmpty;
    }

    public bool IsStackable()
    {
        return stackSize > 1;
    }
}
