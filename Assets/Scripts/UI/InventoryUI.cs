using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;

public class InventoryUI : MonoBehaviour, IBindableUI
{
    [Header("UI Setup")]
    public GameObject itemSlotPrefab;
    public RectTransform slotContainer;

    private Inventory inventory;

    private readonly List<ItemSlotUI> slots = new();
    private ItemSlotUI selectedSlot;

    public void Bind(object inv)
    {
        if(inv.GetType() != typeof(Inventory))
        {
            Debug.LogError("InventoryUI can only bind to Inventory instances.");
            return;
        }
        var inventory = (Inventory)inv;
        Debug.Log($"[InventoryUI] Binding to inventory on {inventory.gameObject.name}");
        if (this.inventory != null)
            this.inventory.UpdateInventory -= Redraw;

        this.inventory = inventory;

        if (inventory != null)
        {
            inventory.UpdateInventory -= Redraw;
            inventory.UpdateInventory += Redraw;
            Redraw();
        }
    }

    private void OnEnable()
    {
        Debug.Log("[InventoryUI] OnEnable called");

        if (inventory != null)
            inventory.UpdateInventory += Redraw;
    }

    private void OnDisable()
    {
        if (inventory != null)
            inventory.UpdateInventory -= Redraw;
    }

    public void Redraw()
    {
        // Clear
        foreach (Transform child in slotContainer)
            Destroy(child.gameObject);
        slots.Clear();

        // Create new ones
        foreach (var invSlot in inventory.slots)
        {
            var ui = Instantiate(itemSlotPrefab, slotContainer);
            var slot = ui.GetComponent<ItemSlotUI>();
            slot.SetStack(invSlot.stack);
            slot.highlight.enabled = false;
            slot.slotType = "inventory";

            var binding = new ItemSlotBinding(
                () => invSlot.stack,
                newStack => invSlot.stack = newStack
            );

            slot.Set(binding);
            slot.OnClicked += HandleSlotClicked;
            slots.Add(slot);
        }
    }

    private void HandleSlotClicked(ItemSlotUI slot)
    {
        selectedSlot = slot;
        foreach (var invSlot in slots)
            invSlot.highlight.enabled = false;
        slot.highlight.enabled = true;

        // Let an external UI (e.g. CampfireUI) decide what to do
    }

    public ItemSlotUI GetSelectedSlot() => selectedSlot;
}
