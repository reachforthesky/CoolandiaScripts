using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;

public class InventoryUI : MonoBehaviour
{
    public GameObject inventoryPanel;
    public GameObject itemSlotPrefab;
    public RectTransform slotContainer;
    public Inventory playerInventory;


    [SerializeField] private KeyCode toggleKey = KeyCode.I;

    private bool isVisible = false;
    private List<ItemSlotUI> slots = new();
    private ItemSlotUI selectedSlot = null;

    void Start()
    {
        playerInventory.UpdateInventory += Redraw;
        Redraw();
    }

    public void Update()
    {
        if (Input.GetKeyDown(toggleKey))
        {
            ToggleInventory();
        }

        if (selectedSlot != null && (Input.GetKey(KeyCode.LeftAlt) || Input.GetKey(KeyCode.RightAlt)))
        {
            for (int i = 0; i < 9; i++)
            {
                if (Input.GetKeyDown(KeyCode.Alpha1 + i))
                {
                    TryAssignToToolbelt(i, selectedSlot);
                    selectedSlot.highlight.enabled = false;
                    selectedSlot = null; // clear after handling
                    break;
                }
            }
        }
    }

    public void Redraw()
    {
        // Clear old slots
        foreach (Transform child in slotContainer)
            Destroy(child.gameObject);
        slots.Clear();
        // Create new ones
        foreach (var invSlot in playerInventory.slots)
        {
            var ui = Instantiate(itemSlotPrefab, slotContainer);
            var slot = ui.GetComponent<ItemSlotUI>();
            slot.SetStack(invSlot.stack);
            slot.slotType = "inventory";
            slot.highlight.enabled = false;
            slot.OnClicked += HandleSlotClicked;
            var binding = new ItemSlotBinding(
                () => invSlot.stack,
                newStack => invSlot.stack = newStack
            );

            slot.Set(binding);
            slots.Add(slot);
        }
    }

    private void HandleSlotClicked(ItemSlotUI slot)
    {
        selectedSlot = slot;
        foreach(var invSlot in slots)
        {
            invSlot.highlight.enabled = false;
        }
        slot.highlight.enabled = true;
    }
    private void TryAssignToToolbelt(int toolbeltIndex, ItemSlotUI inventorySlot)
    {
        var toolbelt = FindObjectOfType<Toolbelt>();
        var temp = toolbelt.GetStack(toolbeltIndex);

        toolbelt.SetStack(toolbeltIndex, inventorySlot.binding.GetStack());
        inventorySlot.binding.SetStack(ItemStack.Empty());

        playerInventory.AddItem(temp);
        Redraw();
    }
    public void ToggleInventory()
    {
        isVisible = !isVisible;
        inventoryPanel.SetActive(isVisible);

        // Optional: Lock/unlock player controls or mouse here
        Cursor.visible = isVisible;
        Cursor.lockState = isVisible ? CursorLockMode.None : CursorLockMode.Locked;
    }
}

