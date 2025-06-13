using UnityEngine;
using System.Collections.Generic;

public class EntityData : MonoBehaviour
{
    [Header("Entity Properties")]
    public int health = 1;
    public ToolType effectiveTool = ToolType.None;

    [Header("Drops")]
    public List<ItemData> drops = new List<ItemData>();

    [Header("Corpse")]
    public EntityData corpsePrefab;

    /// <summary>
    /// Called when an item (e.g., axe, pickaxe) is used on this entity.
    /// </summary>
    /// <param name="usedItem">The item the player/tool used.</param>
    public void ReceiveHit(ItemData usedItem)
    {
        ToolType tool = usedItem.toolType;

        if (effectiveTool == ToolType.None || tool == effectiveTool)
        {
            health--;
            Debug.Log($"{name} was hit with a {tool}. Remaining health: {health}");

            if (health <= 0)
            {
                DestroyEntity();
            }
        }
        else
        {
            Debug.Log($"{name} is unaffected by {tool}.");
        }
    }

    private void DestroyEntity()
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