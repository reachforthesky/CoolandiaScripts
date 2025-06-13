using UnityEngine;
using System;
using NUnit.Framework;
using System.Collections.Generic;

public class Toolbelt : MonoBehaviour
{
    [Header("Toolbelt Settings")]
    public List<InventorySlot> slots;

    public int selectedSlotIndex { get; private set; } = -1;
    public int slotCount = 0;

    private PlayerEquipment equipment;

    public event Action<int, ItemData> OnSlotChanged; // UI hook
    public event Action OnToolbeltResize; 
    public event Action OnContentsChanged;

    void Start()
    {
        equipment = GetComponent<PlayerEquipment>();


        // Set default size if empty
        if (slots == null || slots.Count == 0)
        {
            slots = new List<InventorySlot>();
            for (int i = 0; i < slotCount; i++)
                slots.Add(new InventorySlot(new ItemStack(null, 0)));
        }

        // Equip the first item by default if any
        if (slots.Count > 0)
            EquipSlot(0);

        OnToolbeltResize?.Invoke();
    }

    void Update()
    {
        for (int i = 0; i < slots.Count && i < 9; i++) // Max 9 keys (1–9)
        {
            if (Input.GetKeyDown(KeyCode.Alpha1 + i))
            {
                EquipSlot(i);
                break;
            }
        }
    }

    public void EquipSlot(int index)
    {
        if (index < 0 || index >= slots.Count) return;

        ItemData item = slots[index].stack.item;
        selectedSlotIndex = index;

        if (item != null)
        {
            Debug.Log($"Equipped slot {index + 1}: {item.itemName}");
        }
        else
        {
            Debug.Log($"Slot {index + 1} is empty.");
        }

        OnSlotChanged?.Invoke(index, item);
    }

    public void updateToolbeltSize(int newSize)
    {
        if(newSize > slotCount)
        {
            for(int i = slotCount; i < newSize; i++)
            {
                slots.Add(new InventorySlot(new ItemStack(null, 0)));
            }
        }
        else if(newSize < slotCount) {
            for (int i = slotCount - 1; i < newSize; i++)
            {
                slots.RemoveAt(slots.Count - 1);
            }
        }
        slotCount = slots.Count;
        OnToolbeltResize?.Invoke();
    }

    public void SetStack(int index, ItemStack stack)
    {
        if (index < 0 || index >= slots.Count) return;
        slots[index].stack = stack;
        OnContentsChanged?.Invoke();
    }

    public ItemStack GetStack(int index)
    {
        return (index >= 0 && index < slots.Count) ? slots[index].stack : null;
    }

    public ItemData getEqippedItem()
    {
        return slots[selectedSlotIndex].stack.item;
    }
    public int GetSlotCount() => slots.Count;
}
