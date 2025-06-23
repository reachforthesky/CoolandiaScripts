using UnityEngine;
using Unity.Netcode;

public class VisualVariantSelector : NetworkBehaviour
{
    [SerializeField] private VisualVariantSet variantSet;
    [SerializeField] private SpriteRenderer targetRenderer;

    private int variantIndex = 0;

    public override void OnNetworkSpawn()
    {
        // Auto-find SpriteRenderer in children if not manually assigned
        if (targetRenderer == null)
            targetRenderer = GetComponentInChildren<SpriteRenderer>();

        if (targetRenderer == null)
        {
            Debug.LogWarning($"[{name}] No SpriteRenderer found for visual variant selection.");
            return;
        }

        if (IsServer)
        {
            if (variantSet != null && variantSet.variants.Length > 0)
                variantIndex = Random.Range(0, variantSet.variants.Length);
        }

        ApplyVariant(variantIndex);
    }

    private void ApplyVariant(int index)
    {
        if (variantSet == null || variantSet.variants.Length == 0 || targetRenderer == null)
            return;

        if (index >= 0 && index < variantSet.variants.Length)
        {
            targetRenderer.sprite = variantSet.variants[index];
        }
    }
}
