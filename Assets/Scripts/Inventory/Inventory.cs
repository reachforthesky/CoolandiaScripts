using System;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;

public class Inventory : NetworkBehaviour
{
    public int maxSlots = 20;
    public NetworkList<ItemStack> syncedSlots = new();
    public List<ItemData> acceptedItems = new();

    public event Action UpdateInventory;
    public List<InventorySlot> slots; // wrapper for UI binding
    public override void OnNetworkSpawn()
    {
        // Initialize network list
        if (syncedSlots == null)
            syncedSlots = new NetworkList<ItemStack>();

        // Host initializes slots
        if (IsServer && syncedSlots.Count == 0)
        {
            for (int i = 0; i < maxSlots; i++)
                if (i < slots.Count && !slots[i].stack.IsEmpty())
                    syncedSlots.Add(slots[i].stack);
                else
                    syncedSlots.Add(ItemStack.Empty());
        }

        // Always subscribe to changes
        syncedSlots.OnListChanged += OnSyncedSlotsChanged;
        RebuildSlotsFromNetworkList();
    }

    new private void OnDestroy()
    {
        if (syncedSlots != null)
            syncedSlots.OnListChanged -= OnSyncedSlotsChanged;
    }

    private void OnSyncedSlotsChanged(NetworkListEvent<ItemStack> change)
    {
        RebuildSlotsFromNetworkList();
        UpdateInventory?.Invoke();
    }

    private void RebuildSlotsFromNetworkList()
    {
        // Rebuild non-synced wrapper list
        slots = new List<InventorySlot>(maxSlots);
        foreach (var stack in syncedSlots)
        {
            slots.Add(new InventorySlot(stack));
        }
    }

    public void SetSlot(int index, ItemStack stack)
    {
        if (!IsServer) return;
        if (index < 0 || index >= syncedSlots.Count) return;
        syncedSlots[index] = stack.Clone();
    }

    public void AddItem(ItemStack stack)
    {
        if (!IsServer) return;
        if (stack.itemId == "") return;

        var item = GameDatabaseManager.Instance.Items[stack.itemId];
        if (!Accepts(stack.itemId))
        {
            Debug.LogWarning($"[Inventory] Item {item.itemName} not accepted.");
            return;
        }

        if (item.stackSize>1)
        {
            for (int i = 0; i < syncedSlots.Count; i++)
            {
                var s = syncedSlots[i];
                if (s.itemId == stack.itemId)
                {
                    s.quantity += stack.quantity;
                    syncedSlots[i] = s;
                    return;
                }
            }
        }

        for (int i = 0; i < syncedSlots.Count; i++)
        {
            if (syncedSlots[i].itemId == "")
            {
                syncedSlots[i] = stack.Clone();
                return;
            }
        }

        Debug.LogWarning("[Inventory] Inventory full!");
    }

    public bool RemoveStack(ItemStack stack)
    {
        if (!IsServer) return false;

        int toRemove = stack.quantity;
        var removalPlan = new List<(int index, int amount)>();

        for (int i = 0; i < syncedSlots.Count && toRemove > 0; i++)
        {
            var current = syncedSlots[i];
            if (current.itemId == stack.itemId && current.quantity > 0)
            {
                int amount = Mathf.Min(current.quantity, toRemove);
                removalPlan.Add((i, amount));
                toRemove -= amount;
            }
        }

        if (toRemove > 0)
            return false;

        foreach (var (index, amount) in removalPlan)
        {
            var current = syncedSlots[index];
            current.ReduceBy(amount);
            syncedSlots[index] = current;
        }

        return true;
    }

    public int Count(FixedString32Bytes itemId)
    {
        int total = 0;
        foreach (var stack in syncedSlots)
            if (stack.itemId == itemId)
                total += stack.quantity;
        return total;
    }

    public bool Has(ItemStack stack) => Count(stack.itemId) >= stack.quantity;

    public bool Accepts(FixedString32Bytes itemId)
    {
        var item = GameDatabaseManager.Instance.Items[itemId];
        return acceptedItems.Count == 0 || acceptedItems.Contains(item);
    }
    [ServerRpc(RequireOwnership = false)]
    public void RequestSetSlotServerRpc(int index, FixedString32Bytes itemId, int quantity)
    {
        if (index < 0 || index >= syncedSlots.Count) return;
        syncedSlots[index] = new ItemStack(itemId, quantity);
    }

    [ServerRpc(RequireOwnership = false)]
    public void RequestAddItemServerRpc(string itemId, int quantity)
    {
        var item = new ItemStack(itemId, quantity);
        AddItem(item);
    }

    [ServerRpc(RequireOwnership = false)]
    public void RequestRemoveItemServerRpc(string itemId, int quantity)
    {
        var item = new ItemStack(itemId, quantity);
        RemoveStack(item);
    }
}
