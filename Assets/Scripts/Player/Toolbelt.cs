using UnityEngine;
using System;
using System.Collections.Generic;

public class Toolbelt : MonoBehaviour
{
    [Header("Toolbelt Settings")]
    [SerializeField] private List<InventorySlot> slots = new();
    [SerializeField] private int slotCount = 5;

    public int selectedSlotIndex { get; private set; } = -1;


    public event Action<int, ItemData> OnSlotChanged;
    public event Action OnToolbeltResize;
    public event Action OnContentsChanged;

    private void Start()
    {

        InitializeSlots(slotCount);

        if (slots.Count > 0)
            EquipSlot(0);

        OnToolbeltResize?.Invoke();
    }

    private void Update()
    {
        for (int i = 0; i < slots.Count && i < 9; i++) // 1-9 keypress
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
        slots ??= new List<InventorySlot>();

        while (slots.Count < size)
            slots.Add(new InventorySlot(new ItemStack(null, 0)));

        slotCount = slots.Count;
    }

    public void EquipSlot(int index)
    {
        if (index < 0 || index >= slots.Count) return;

        selectedSlotIndex = index;
        ItemData item = slots[index].stack.item;

        Debug.Log(item != null
            ? $"Equipped slot {index + 1}: {item.itemName}"
            : $"Slot {index + 1} is empty.");

        OnSlotChanged?.Invoke(index, item);
    }

    public void UpdateToolbeltSize(int newSize)
    {
        if (newSize > slotCount)
        {
            for (int i = slotCount; i < newSize; i++)
                slots.Add(new InventorySlot(new ItemStack(null, 0)));
        }
        else if (newSize < slotCount)
        {
            slots.RemoveRange(newSize, slotCount - newSize);
        }

        slotCount = slots.Count;
        OnToolbeltResize?.Invoke();
    }

    public void SetStack(int index, ItemStack stack)
    {
        if (index < 0 || index >= slots.Count) return;

        slots[index].stack = stack;
        OnContentsChanged?.Invoke();

        if (index == selectedSlotIndex)
            OnSlotChanged?.Invoke(index, stack.item);
    }

    public ItemStack GetStack(int index)
    {
        return (index >= 0 && index < slots.Count)
            ? slots[index].stack
            : ItemStack.Empty();
    }

    public InventorySlot GetSlot(int index)
    {
        return (index >= 0 && index < slots.Count)
            ? slots[index]
            : null;
    }
    public int GetSlotCount() => slots.Count;

    public ItemData GetEquippedItem()
    {
        return (selectedSlotIndex >= 0 && selectedSlotIndex < slots.Count)
            ? slots[selectedSlotIndex].stack.item
            : null;
    }
}
