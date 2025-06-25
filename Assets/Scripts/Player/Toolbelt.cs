using UnityEngine;
using System;
using System.Collections.Generic;

public class Toolbelt : MonoBehaviour
{
    [Header("Toolbelt Settings")]
    public Inventory toolbeltInventory;
    [SerializeField] private int slotCount = 5;

    public int selectedSlotIndex { get; private set; } = -1;


    public event Action<int, int> OnSlotChanged;
    public event Action OnToolbeltResize;
    public event Action OnContentsChanged;

    private void Start()
    {

        InitializeSlots(slotCount);

        if (toolbeltInventory.slots.Count > 0)
            EquipSlot(0);

        OnToolbeltResize?.Invoke();
    }

    private void Update()
    {
        for (int i = 0; i < toolbeltInventory.slots.Count && i < 9; i++) // 1-9 keypress
        {
            if (Input.GetKeyDown(KeyCode.Alpha1 + i))
            {
                EquipSlot(i);
                break;
            }
        }
    }

    private void InitializeSlots(int size)
    {
        toolbeltInventory.slots ??= new List<InventorySlot>();

        while (toolbeltInventory.slots.Count < size)
            toolbeltInventory.slots.Add(new InventorySlot(ItemStack.Empty()));

        slotCount = toolbeltInventory.slots.Count;
    }

    public void EquipSlot(int index)
    {
        if (index < 0 || index >= toolbeltInventory.slots.Count) return;

        selectedSlotIndex = index;
        var itemId = toolbeltInventory.slots[index].stack.itemId;
        ItemData item = GameDatabaseManager.Instance.Items.Get(itemId);

        Debug.Log(item != null
            ? $"Equipped slot {index + 1}: {item.itemName}"
            : $"Slot {index + 1} is empty.");

        var id = item != null
            ? item.itemId
            : 0;

        OnSlotChanged?.Invoke(index, id);
    }

    public void UpdateToolbeltSize(int newSize)
    {
        if (newSize > slotCount)
        {
            for (int i = slotCount; i < newSize; i++)
                toolbeltInventory.slots.Add(new InventorySlot(ItemStack.Empty()));
        }
        else if (newSize < slotCount)
        {
            toolbeltInventory.slots.RemoveRange(newSize, slotCount - newSize);
        }

        slotCount = toolbeltInventory.slots.Count;
        OnToolbeltResize?.Invoke();
    }

    public void SetStack(int index, ItemStack stack)
    {
        if (index < 0 || index >= toolbeltInventory.slots.Count) return;

        toolbeltInventory.slots[index].stack = stack;
        OnContentsChanged?.Invoke();

        if (index == selectedSlotIndex)
            OnSlotChanged?.Invoke(index, stack.itemId);
    }

    public ItemStack GetStack(int index)
    {
        return (index >= 0 && index < toolbeltInventory.slots.Count)
            ? toolbeltInventory.slots[index].stack
            : ItemStack.Empty();
    }

    public InventorySlot GetSlot(int index)
    {
        return (index >= 0 && index < toolbeltInventory.slots.Count)
            ? toolbeltInventory.slots[index]
            : null;
    }
    public int GetSlotCount() => toolbeltInventory.slots.Count;

    public ItemData GetEquippedItem()
    {
        return (selectedSlotIndex >= 0 && selectedSlotIndex < toolbeltInventory.slots.Count)
            ? GameDatabaseManager.Instance.Items.Get(toolbeltInventory.slots[selectedSlotIndex].stack.itemId)
            : null;
    }
}
