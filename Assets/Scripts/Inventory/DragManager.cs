using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class DragManager : MonoBehaviour
{
    public static DragManager Instance { get; set; }

    [SerializeField] private GameObject dragIconPrefab;
    [SerializeField] private Canvas canvas;
    private GameObject dragIconInstance;
    private Image dragImage;
    private ItemStack draggingStack;
    private ItemSlotUI originalSlot;


    public bool IsDragging => !draggingStack.IsEmpty();


    private void Awake()
    {
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

        dragImage.sprite = ItemDatabase.Instance.Get(draggingStack.itemId).icon;
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
        draggingStack = ItemStack.Empty();
        return result;
    }

    public void HandleDrop(ItemSlotUI sourceSlot, ItemSlotUI targetSlot)
    {
        var dragStack = draggingStack;
        var targetStack = targetSlot.binding.GetStack();

        if (targetStack.IsEmpty())
        {
            targetSlot.binding.RequestSet(dragStack);
            sourceSlot.binding.RequestSet(ItemStack.Empty());
        }
        else if (targetStack.itemId == dragStack.itemId && ItemDatabase.Instance.Get(dragStack.itemId).isStackable)
        {
            var merged = new ItemStack(dragStack.itemId, targetStack.quantity + dragStack.quantity);
            targetSlot.binding.RequestSet(merged);
            sourceSlot.binding.RequestSet(ItemStack.Empty());
        }
        else
        {
            targetSlot.binding.RequestSet(dragStack);
            sourceSlot.binding.RequestSet(targetStack);
        }

        EndDrag();
        sourceSlot.Redraw();
        targetSlot.Redraw();
    }
}
