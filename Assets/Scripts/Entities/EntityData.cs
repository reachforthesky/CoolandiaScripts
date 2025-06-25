using System;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using static UnityEngine.EventSystems.EventTrigger;

public class EntityData : NetworkBehaviour, IItemUsable, IInteractable
{
    [Header("Entity Properties")]
    public Tag DestructiveTool;

    [Header("Drops")]
    public List<ItemData> drops = new List<ItemData>();

    [Header("Corpse")]
    public EntityData corpsePrefab;

    private StatHandler statHandler;

    public event Action<int> receiveHit;
    public event Action<ItemData> itemUsed;
    public event Action<PlayerController> interacted;
    public int persistentIndex = -1;
    ulong IInteractable.NetworkObjectId
    {
        get => base.NetworkObjectId;
        set { /* NetworkObjectId is readonly in NetworkBehaviour, so setter is intentionally left empty */ }
    }

    void Awake()
    {
        statHandler = GetComponent<StatHandler>();
    }

    [ServerRpc(RequireOwnership = false)]
    public void UseItemServerRpc(int itemId)
    {
        var item = GameDatabaseManager.Instance.Items.Get(itemId);
        ItemUsed(item);
    }
    /// <summary>
    /// Called when an item (e.g., axe, pickaxe) is used on this entity.
    /// </summary>
    /// <param name="usedItem">The item the player/tool used.</param>
    public virtual void ItemUsed(ItemData usedItem)
    {
        var tags = usedItem.tags;

        if ( DestructiveTool == Tag.All || tags.Contains(DestructiveTool)) 
        {
            if (statHandler && statHandler.stats.ContainsKey(Stat.Health))
            {
                statHandler.stats[Stat.Health]-= usedItem.damage;
                Debug.Log($"{name} was hit with a {usedItem.name}. Remaining health: {statHandler.stats[Stat.Health]}");
            }
            receiveHit?.Invoke(usedItem.damage);
        }
        else
        {
            Debug.Log($"{name} is unaffected by {usedItem.name}.");
        }
        itemUsed?.Invoke(usedItem);
    }

    public void Interact(PlayerController player)
    {
        Debug.Log($"Interacted with {name}.");
        // Implement interaction logic here, if needed
        interacted?.Invoke(player);
    }

    public void DestroyEntity()
    {
        if (!IsServer)
            return;

        Debug.Log($"{name} destroyed! {OwnerClientId}");

        // Drop items
        foreach (var drop in drops)
        {
            DropSpawner.Instance?.SpawnDrop(drop, transform.position);
        }

        // Spawn corpse (if any)
        if (corpsePrefab != null)
        {
            PersistentEntityData persistentData = new PersistentEntityData
            {
                prefabId = Array.IndexOf(PersistentEntityManager.Instance.entityPrefabs, corpsePrefab),
                position = this.transform.position,
                rotation = this.transform.rotation,
                isDestroyed = false
            };

            PersistentEntityManager.Instance.RegisterEntity(persistentData);
        }
        PersistentEntityManager.Instance.MarkDestroyed(persistentIndex);
    }
}