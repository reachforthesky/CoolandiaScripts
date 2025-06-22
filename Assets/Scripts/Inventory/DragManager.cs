using UnityEngine;
using UnityEngine.UI;

public class DragManager : MonoBehaviour
{
    public static DragManager Instance { get; private set; }

    [SerializeField] private GameObject dragIconPrefab;
    [SerializeField] private Canvas canvas;
    private GameObject dragIconInstance;
    private Image dragImage;
    private ItemStack draggingStack;
    private ItemSlotUI originalSlot;


    public bool IsDragging => draggingStack != null && !draggingStack.IsEmpty();

    private void Awake()
    {
        if (Instance != null) Destroy(gameObject);
        Instance = this;

        dragIconInstance = Instantiate(dragIconPrefab, canvas.transform); 
        dragIconInstance.transform.SetAsLastSibling();
        dragImage = dragIconInstance.GetComponent<Image>();
        dragImage.raycastTarget = false;
        dragIconInstance.SetActive(false);
    }

    public void BeginDrag(ItemSlotUI fromSlot, ItemStack stack)
    {
        dragIconInstance.transform.SetAsLastSibling();
        originalSlot = fromSlot;
        draggingStack = stack.Clone();

        dragImage.sprite = draggingStack.item.icon;
        dragIconInstance.SetActive(true);
    }

    public void UpdateDragPosition(Vector2 screenPos)
    {
        RectTransform canvasRect = canvas.transform as RectTransform;
        RectTransform iconRect = dragIconInstance.GetComponent<RectTransform>();

        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRect, screenPos, null, out Vector2 localPos))
        {
            iconRect.localPosition = localPos;
        }
        Debug.Log($"[DragManager] Updating position: {screenPos}");
    }

    public ItemStack EndDrag()
    {
        dragIconInstance.SetActive(false);
        var result = draggingStack;
        draggingStack = null;
        return result;
    }

    public void HandleDrop(ItemSlotUI sourceSlot, ItemSlotUI targetSlot)
    {
        var targetStack = targetSlot.binding.GetStack();

        if (targetStack.IsEmpty())
        {
            // Move to empty slot
            targetSlot.binding.SetStack(draggingStack);
            EndDrag();
            sourceSlot.Redraw();
            targetSlot.Redraw();
        }
        else if (targetStack.item == draggingStack.item && targetStack.item.isStackable)
        {
            // Merge stacks
            targetStack.quantity += draggingStack.quantity;
            targetSlot.binding.SetStack(targetStack);
            EndDrag();
            sourceSlot.Redraw();
            targetSlot.Redraw();
        }
        else
        {
            // Swap
            targetSlot.binding.SetStack(draggingStack);
            sourceSlot.binding.SetStack(targetStack);
            EndDrag();
            sourceSlot.Redraw();
            targetSlot.Redraw();
        }
    }
}
