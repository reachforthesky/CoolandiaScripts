using UnityEngine;

public class VisualVariantSelector : MonoBehaviour
{
    [SerializeField] private VisualVariantSet variantSet;

    // Drag the child transform manually, or find it automatically
    [SerializeField] private SpriteRenderer targetRenderer;

    private void Awake()
    {
        if (variantSet == null || variantSet.variants.Length == 0)
            return;

        // Auto-find SpriteRenderer in children if not manually assigned
        if (targetRenderer == null)
            targetRenderer = GetComponentInChildren<SpriteRenderer>();

        if (targetRenderer == null)
        {
            Debug.LogWarning($"[{name}] No SpriteRenderer found for visual variant selection.");
            return;
        }

        int index = Random.Range(0, variantSet.variants.Length);
        targetRenderer.sprite = variantSet.variants[index];
    }
}
