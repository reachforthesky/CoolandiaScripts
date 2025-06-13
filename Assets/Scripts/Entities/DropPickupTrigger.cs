using UnityEngine;

public class DropPickupTrigger : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        ItemDrop drop = GetComponentInParent<ItemDrop>();
        if (drop != null)
        {
            drop.TryPickup(other.gameObject);
        }
    }
}