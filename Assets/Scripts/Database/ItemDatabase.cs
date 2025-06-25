using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "ItemDatabase", menuName = "Databases/ItemDatabase")]
public class ItemDatabase : ScriptableObject
{
    [SerializeField] private List<ItemData> items = new();

    private Dictionary<int, ItemData> idToItem = new();

    public void Initialize()
    {
        idToItem.Clear();
        int i = 1;
        foreach (var item in items)
        {
            if (item == null) continue;
            item.itemId = i;
            idToItem[i] = item;
            i++;
        }
    }

    public ItemData Get(int id)
    {
        idToItem.TryGetValue(id, out var item);
        return item;
    }

    public List<ItemData> AllItems => items;
}
