using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ToolbeltUI : MonoBehaviour
{
    [Header("References")]
    public Toolbelt toolbelt;
    public GameObject slotPrefab; // Should be your shared ItemSlotUI prefab
    public RectTransform slotContainer; // Horizontal Layout Group or Grid

    private List<ItemSlotUI> slotUIs = new();

    void Start()
    {
        // No more automatic toolbelt = FindFirstObjectByType<Toolbelt>();
        if (toolbelt != null)
        {
            Bind(toolbelt); // still useful for manual testing
        }
    }
    public void Bind(Toolbelt tb)
    {
        if (toolbelt != null)
        {
            // Unhook old listeners
            toolbelt.OnSlotChanged -= UpdateHighlight;
            toolbelt.OnToolbeltResize -= Refresh;
            toolbelt.OnContentsChanged -= Refresh;
        }

        toolbelt = tb;

        toolbelt.OnSlotChanged += UpdateHighlight;
        toolbelt.OnToolbeltResize += Refresh;
        toolbelt.OnContentsChanged += Refresh;

        RebuildSlots();
    }
    private void RebuildSlots()
    {
        if (DragManager.Instance != null && DragManager.Instance.IsDragging)
            return;
        // Clear old slots
        foreach (Transform child in slotContainer)
            Destroy(child.gameObject);
        slotUIs.Clear();

        for (int i = 0; i < toolbelt.GetSlotCount(); i++)
        {
            GameObject slotGO = Instantiate(slotPrefab, slotContainer);
            var slotUI = slotGO.GetComponent<ItemSlotUI>();

            var index = i; // Capture for lambda
            var binding = new ItemSlotBinding(
                () => toolbelt.GetStack(index),
                index,
                toolbelt.toolbeltInventory
            );

            slotUI.Index = index;
            slotUI.slotType = "toolbelt";
            slotUI.Set(binding);

            slotUI.OnClicked += (clickedSlot) =>
            {
                toolbelt.EquipSlot(clickedSlot.Index);
            };

            // Highlight if currently selected
            slotUI.highlight.enabled = (index == toolbelt.selectedSlotIndex);

            slotUIs.Add(slotUI);
        }
        Canvas.ForceUpdateCanvases();
        LayoutRebuilder.ForceRebuildLayoutImmediate(slotContainer);
    }

    private void UpdateHighlight(int selectedIndex, int itemId)
    {
        for (int i = 0; i < slotUIs.Count; i++)
        {
            slotUIs[i].highlight.enabled = (i == selectedIndex);
        }
    }

    public void Refresh()
    {
        RebuildSlots();
    }
}
