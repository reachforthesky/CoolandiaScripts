using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;
using System;

public class ItemSlotUI : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
{
    [Header("UI References")]
    public Image background;
    public Image icon;
    public TextMeshProUGUI stackText;
    public Image highlight;


    public ItemSlotBinding binding; 

    [Header("Metadata")]
    public string slotType; // Optional: "toolbelt", "inventory", etc.

    public ItemStack currentItem; // The data it holds

    public event System.Action<ItemSlotUI> OnClicked;
    public event System.Action<ItemSlotUI> OnHovered;
    public event System.Action<ItemSlotUI> OnUnhovered;

    public int index { get; set; }
    public void Set(ItemSlotBinding newBinding)
    {
        binding = newBinding;
        UpdateUI();
    }

    public void UpdateUI()
    {
        var stack = binding?.GetStack() ?? ItemStack.Empty();
        icon.enabled = stack.item;
        icon.sprite = stack.item?.icon;
        stackText.text = stack.quantity > 1 ? stack.quantity.ToString() : "";
    }
    public void SetStack(ItemStack stack)
    {
        currentItem = stack;

        if (stack == null || stack.item == null)
        {
            icon.enabled = false;
            stackText.text = "";
        }
        else
        {
            icon.enabled = true;
            icon.sprite = stack.item.icon;
            stackText.text = stack.quantity > 1 ? stack.quantity.ToString() : "";
        }
    }

    public void OnPointerClick(PointerEventData eventData) => OnClicked?.Invoke(this);
    public void OnPointerEnter(PointerEventData eventData) => OnHovered?.Invoke(this);
    public void OnPointerExit(PointerEventData eventData) => OnUnhovered?.Invoke(this);
}
public class ItemSlotBinding
{
    public Func<ItemStack> GetStack;
    public Action<ItemStack> SetStack;

    public ItemSlotBinding(Func<ItemStack> getter, Action<ItemStack> setter)
    {
        GetStack = getter;
        SetStack = setter;
    }
}
