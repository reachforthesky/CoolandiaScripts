using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UIElements;

public class DragManager : MonoBehaviour
{
    public static DragManager Instance { get; set; }

    [SerializeField] private GameObject dragIconPrefab;
    [SerializeField] private Canvas canvas;
    private GameObject dragIconInstance;
    private UnityEngine.UI.Image dragImage;
    private ItemStack draggingStack;
    private ItemSlotUI originalSlot;


    public bool IsDragging => !draggingStack.IsEmpty();


    private void Awake()
    {
        dragIconInstance = Instantiate(dragIconPrefab, canvas.transform);
        dragIconInstance.transform.SetAsLastSibling();
        dragImage = dragIconInstance.GetComponent<UnityEngine.UI.Image>();
        dragImage.raycastTarget = false;
        dragIconInstance.SetActive(false);
    }

    public void BeginDrag(ItemSlotUI fromSlot, ItemStack stack)
    {
        dragIconInstance.transform.SetAsLastSibling();
        originalSlot = fromSlot;
        draggingStack = stack.Clone();

        var iconId = GameDatabaseManager.Instance.Items[draggingStack.itemId].iconId;
        dragImage.sprite = GameDatabaseManager.Instance.Sprites[draggingStack.itemId];
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
        var dragStack = draggingStack; // quantity to move
        var sourceStack = sourceSlot.binding.GetStack(); // full original stack
        var targetStack = targetSlot.binding.GetStack();

        int remainingInSource = sourceStack.quantity - dragStack.quantity;

        // --- Merge ---
        if (!targetStack.IsEmpty() && targetStack.itemId == dragStack.itemId &&
            GameDatabaseManager.Instance.Items[dragStack.itemId].IsStackable())
        {
            var merged = new ItemStack(dragStack.itemId, targetStack.quantity + dragStack.quantity);
            targetSlot.binding.RequestSet(merged);
            sourceSlot.binding.RequestSet(remainingInSource > 0
                ? new ItemStack(sourceStack.itemId, remainingInSource)
                : ItemStack.Empty());
        }

        // --- Empty slot ---
        else if (targetStack.IsEmpty())
        {
            targetSlot.binding.RequestSet(dragStack);
            sourceSlot.binding.RequestSet(remainingInSource > 0
                ? new ItemStack(sourceStack.itemId, remainingInSource)
                : ItemStack.Empty());
        }

        // --- Swap (with partial source stack) ---
        else
        {
            // Swap only valid if full stack dragged, otherwise reinsert source portion
            if (dragStack.quantity == sourceStack.quantity)
            {
                // Full-stack swap: safe
                targetSlot.binding.RequestSet(dragStack);
                sourceSlot.binding.RequestSet(targetStack);
            }
            else
            {
                // Partial-stack swap: can't overwrite remaining source stack
                Debug.LogWarning("Can't swap partial stacks with different items.");

                // Optionally cancel or revert drag
                sourceSlot.binding.RequestSet(sourceStack); // restore original
                targetSlot.binding.RequestSet(targetStack); // leave untouched
            }
        }

        EndDrag();
        sourceSlot.Redraw();
        targetSlot.Redraw();
    }
}
