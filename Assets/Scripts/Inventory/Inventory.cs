using System;
using System.Collections.Generic;
using UnityEngine;
using static UnityEditor.Progress;

public class Inventory : MonoBehaviour
{
    public int maxSlots = 20;
    public List<InventorySlot> slots = new();
    public List<ItemData> acceptedItems = new();

    public event Action UpdateInventory;

    public void Start()
    {
        for (int i = slots.Count; i < maxSlots; i++)
        {
            var slot = new InventorySlot(new ItemStack(null, 0));
            slots.Add(slot);
        }
    }

    public bool AddItem(ItemStack stack)
    {
        if (stack == null || stack.item == null)
            return true;

        if (!Accepts(stack.item))
        {
            Debug.Log($"Inventory does not accept {stack.item.name}.");
            return false;
        }

        if (stack.item.isStackable)
        {
            var existingSlot = slots.Find(s => s.Matches(stack.item));
            if (existingSlot != null)
            {
                existingSlot.stack.quantity += stack.quantity;
                Debug.Log("[Inventory] Added item, updating inventory.");
                UpdateInventory?.Invoke();
                return true;
            }
        }

        foreach (var slot in slots)
        {
            if (!slot.stack.item)
            {
                slot.stack = stack.Clone();
                Debug.Log("[Inventory] Added item, updating inventory.");
                UpdateInventory?.Invoke();
                return true;
            }
        }

        Debug.Log("Inventory full!");
        return false;
    }

    public bool RemoveStack(ItemStack stack)
    {
        int toRemove = stack.quantity;
        List<(int index, int amount)> removalPlan = new();

        for (int i = 0; i < slots.Count && toRemove > 0; i++)
        {
            var currentStack = slots[i].stack;
            if (currentStack.item == stack.item && currentStack.quantity > 0)
            {
                int removable = Mathf.Min(currentStack.quantity, toRemove);
                removalPlan.Add((i, removable));
                toRemove -= removable;
            }
        }

        if (toRemove > 0)
        {
            // Not enough items to remove
            return false;
        }

        // Apply removals
        foreach (var (index, amount) in removalPlan)
        {
            slots[index].stack.ReduceBy(amount);
            Debug.Log("[Inventory] Added item, updating inventory.");
            UpdateInventory?.Invoke();
        }

        return true;
    }

    public int Count(ItemData item)
    {
        int accumulator = 0;
        foreach (var slot in slots)
        {
            if (slot.stack.item == item)
                accumulator += slot.stack.quantity;
        }
        return accumulator;
    }

    public bool Has(ItemStack stack)
    {
        return Count(stack.item) >= stack.quantity;
    }

    public bool Accepts(ItemData item)
    {
        return acceptedItems.Count <= 0 || !acceptedItems.Contains(item);
    }
}