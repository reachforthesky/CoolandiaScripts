using System.Collections.Generic;
using UnityEngine;

public class ItemDatabase : MonoBehaviour
{
    public static ItemDatabase Instance { get; private set; }

    [SerializeField] private List<ItemData> items = new();

    private Dictionary<int, ItemData> idToItem = new();

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

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
}
