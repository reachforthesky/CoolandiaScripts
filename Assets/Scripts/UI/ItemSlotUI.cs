using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;
using System;

public class ItemSlotUI : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler, IBeginDragHandler, IDragHandler, IEndDragHandler, IDropHandler
{
    [Header("UI References")]
    public Image background;
    public Image icon;
    public TextMeshProUGUI stackText;
    public Image highlight;
    public TextMeshProUGUI placeholderText;


    public ItemSlotBinding binding; 

    [Header("Metadata")]
    public string slotType; // Optional: "toolbelt", "inventory", etc.

    public ItemStack currentItem; // The data it holds

    public event System.Action<ItemSlotUI> OnClicked;
    public event System.Action<ItemSlotUI> OnHovered;
    public event System.Action<ItemSlotUI> OnUnhovered;

    public int Index { get; set; }
    public void Set(ItemSlotBinding newBinding)
    {
        binding = newBinding;
        Redraw();
    }

    public void OnBeginDrag(PointerEventData eventData)
    {

        var stack = binding.GetStack();
        if (stack.IsEmpty()) return;

        DragManager.Instance.BeginDrag(this, stack); // <-- Use RPC to clear it
        DragManager.Instance.UpdateDragPosition(eventData.position);
    }
    public void OnDrag(PointerEventData eventData)
    {
        Debug.Log($"Dragging: {eventData.position}");
        DragManager.Instance.UpdateDragPosition(eventData.position);
    }
    public void OnEndDrag(PointerEventData eventData)
    {
        if (!DragManager.Instance.IsDragging)
            return;

        if (eventData.pointerEnter != null)
        {
            Debug.Log("Dropped on: " + eventData.pointerEnter.name);
        }
        else
        {
            Debug.Log("Dropped on empty space");
        }


        if (!eventData.pointerEnter || !eventData.pointerEnter.GetComponentInParent<ItemSlotUI>())
        {
            // Didn't drop on a valid slot ? return item to original
            binding.RequestSet(DragManager.Instance.EndDrag());
            return;
        }

        var targetSlot = eventData.pointerEnter?.GetComponentInParent<ItemSlotUI>();

        binding.RequestSet(ItemStack.Empty());
        // Let DragManager resolve drop logic
        DragManager.Instance.HandleDrop(this, targetSlot);
    }
    public void OnDrop(PointerEventData eventData)
    {
        // Required to receive drop, even if logic is in EndDrag
    }

    public void Redraw()
    {
        var stack = binding?.GetStack() ?? ItemStack.Empty();
        var stackItem = ItemDatabase.Instance.Get(stack.itemId);
        icon.enabled = stackItem;
        if(stackItem)
            icon.sprite = stackItem.icon;
        stackText.text = stack.quantity > 1 ? stack.quantity.ToString() : "";
        SetPlaceholder(stackItem); 
    }
    public void SetStack(ItemStack stack)
    {
        currentItem = stack;

        if (stack.itemId == 0)
        {
            icon.enabled = false;
            stackText.text = "";
        }
        else
        {
            icon.enabled = true;
            icon.sprite = ItemDatabase.Instance.Get(stack.itemId).icon;
            stackText.text = stack.quantity > 1 ? stack.quantity.ToString() : "";
        }
    }

    public void SetPlaceholder(ItemData itemData)
    {
        if (!icon.sprite && itemData != null)
        {
            placeholderText.enabled = true;
            placeholderText.text = itemData.name;
        }
        else { placeholderText.enabled = false; }
    }

    public void OnPointerClick(PointerEventData eventData) => OnClicked?.Invoke(this);
    public void OnPointerEnter(PointerEventData eventData) => OnHovered?.Invoke(this);
    public void OnPointerExit(PointerEventData eventData) => OnUnhovered?.Invoke(this);
}
public class ItemSlotBinding
{
    public Func<ItemStack> GetStack;
    public int Index;
    public Inventory OwningInventory;

    public ItemSlotBinding(Func<ItemStack> getter, int index, Inventory owner)
    {
        GetStack = getter;
        Index = index;
        OwningInventory = owner;
    }

    public void RequestSet(ItemStack stack)
    {
        if (OwningInventory.IsServer)
            OwningInventory.SetSlot(Index, stack);
        else
            OwningInventory.RequestSetSlotServerRpc(Index, stack.itemId, stack.quantity);
    }
}
