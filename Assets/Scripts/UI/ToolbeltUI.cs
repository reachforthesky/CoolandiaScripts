using UnityEngine;
using System.Collections.Generic;

public class ToolbeltUI : MonoBehaviour
{
    public Toolbelt toolbelt;
    public GameObject slotPrefab; // The ToolbeltSlotUI prefab
    public Transform slotContainer; // The Horizontal Layout Group container

    private List<ToolbeltSlotUI> slotUIs = new();

    void Start()
    {
        if (toolbelt == null)
            toolbelt = FindFirstObjectByType<Toolbelt>();

        // Rebuild the UI slots based on toolbelt data
        RebuildSlots();

        toolbelt.OnSlotChanged += UpdateHighlight;
        toolbelt.OnToolbeltResize += Refresh;
        toolbelt.OnContentsChanged += Refresh;
    }

    void RebuildSlots()
    {
        // Clean existing children
        foreach (Transform child in slotContainer)
        {
            Destroy(child.gameObject);
        }

        slotUIs.Clear();

        for (int i = 0; i < toolbelt.GetSlotCount(); i++)
        {
            GameObject slotGO = Instantiate(slotPrefab, slotContainer);
            ToolbeltSlotUI slotUI = slotGO.GetComponent<ToolbeltSlotUI>();

            var item = toolbelt.slots[i].stack.item;
            slotUI.SetIcon(item?.icon);
            slotUI.SetActive(i == toolbelt.selectedSlotIndex);

            slotUIs.Add(slotUI);
        }
    }

    void UpdateHighlight(int selectedIndex, ItemData item)
    {
        for (int i = 0; i < slotUIs.Count; i++)
        {
            slotUIs[i].SetActive(i == selectedIndex);
        }
    }

    // Call this when the player unlocks more slots
    public void Refresh()
    {
        RebuildSlots();
    }
}
