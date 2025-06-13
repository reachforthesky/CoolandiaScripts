using UnityEngine;

public class DropSpawner : MonoBehaviour
{
    public static DropSpawner Instance;

    public GameObject dropPrefab; // Assign in inspector

    private void Awake()
    {
        if (Instance != null && Instance != this)
            Destroy(gameObject);
        else
            Instance = this;
    }

    public void SpawnDrop(ItemData item, Vector3 position)
    {
        if (dropPrefab == null || item == null) return;

        GameObject drop = Instantiate(dropPrefab, position + Vector3.up * 0.5f, Quaternion.identity);
        var dropComponent = drop.GetComponent<ItemDrop>();
        dropComponent?.Initialize(item);
    }
}