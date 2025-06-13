using UnityEngine;
using System.Collections.Generic;
using System;

public class EntityData : MonoBehaviour
{
    [Header("Entity Properties")]
    public int health = 1;
    public ToolType effectiveTool = ToolType.None;

    [Header("Drops")]
    public List<ItemData> drops = new List<ItemData>();

    [Header("Corpse")]
    public EntityData corpsePrefab;

    private StatHandler statHandler;

    public event Action<int> receiveHit;

    void Awake()
    {
        statHandler = GetComponent<StatHandler>();
    }
    /// <summary>
    /// Called when an item (e.g., axe, pickaxe) is used on this entity.
    /// </summary>
    /// <param name="usedItem">The item the player/tool used.</param>
    public void ReceiveHit(ItemData usedItem)
    {
        ToolType tool = usedItem.toolType;

        if (effectiveTool == ToolType.None || tool == effectiveTool)
        {
            if (statHandler && statHandler.stats.ContainsKey(Stat.health))
            {
                statHandler.stats[Stat.health]-= usedItem.damage;
                Debug.Log($"{name} was hit with a {tool}. Remaining health: {health}");
            }
            receiveHit?.Invoke(usedItem.damage);
        }
        else
        {
            Debug.Log($"{name} is unaffected by {tool}.");
        }
    }

    public void DestroyEntity()
    {
        Debug.Log($"{name} destroyed!");

        // Drop items
        foreach (var drop in drops)
        {
            DropSpawner.Instance?.SpawnDrop(drop, transform.position);
        }

        // Spawn corpse (if any)
        if (corpsePrefab != null)
        {
            Instantiate(corpsePrefab, transform.position, transform.rotation);
        }

        Destroy(gameObject);
    }
}