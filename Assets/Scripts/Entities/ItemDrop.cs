using NUnit.Framework.Interfaces;
using UnityEngine;
using UnityEngine.UI;

public class ItemDrop : MonoBehaviour
{
    [SerializeField] private float attractionRadius = 3f;
    [SerializeField] private float attractionSpeed = 5f;
    [SerializeField] private float maxSpeed = 10f;

    public SpriteRenderer iconRenderer;
    public float lifetime = 30f;
    public float targetSize = 1f;
    public float verticleOffest = .05f;
    public float collectDelay = 0.3f;

    private Rigidbody rb;
    private Transform playerTransform;
    private float spawnTime;
    private float delayBeforeAttraction = 0.3f;
    private ItemData itemData;

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        spawnTime = Time.time;

        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            playerTransform = player.transform;
        }
        Destroy(gameObject, lifetime);
    }

    public void Initialize(ItemData item)
    {
        itemData = item;
        if (iconRenderer != null && item != null)
        {
            float targetHeight = targetSize; // world units tall
            float spriteHeight = item.icon.bounds.size.y;
            float scale = targetHeight / spriteHeight;

            iconRenderer.sprite = item.icon;

            // Normalize size
            if (item.icon != null)
            {
                iconRenderer.transform.localScale = Vector3.one * scale;
                iconRenderer.transform.localPosition = new Vector3(0f, verticleOffest, 0f);
            }
        }

        // Add random force
        var rb = GetComponent<Rigidbody>();
        if (rb != null)
        {
            Vector3 randomDir = new Vector3(
                Random.Range(-1f, 1f),
                1f,
                Random.Range(-1f, 1f)
            ).normalized;

            rb.AddForce(randomDir * Random.Range(2f, 5f), ForceMode.Impulse);
        }
    }

    void FixedUpdate()
    {
        if (playerTransform == null || rb == null) return;
        if (Time.time - spawnTime < delayBeforeAttraction) return;

        Vector3 toPlayer = playerTransform.position - transform.position;
        float distance = toPlayer.magnitude;

        if (distance <= attractionRadius)
        {
            Vector3 forceDir = toPlayer.normalized;
            float forceMagnitude = Mathf.Lerp(0f, attractionSpeed, 1f - (distance / attractionRadius));

            Vector3 force = forceDir * forceMagnitude;

            // Clamp total speed
            if (rb.linearVelocity.magnitude < maxSpeed)
            {
                rb.AddForce(force, ForceMode.Acceleration);
            }
        }
    }

    public void TryPickup(GameObject collector)
    {
        if (Time.time - spawnTime < collectDelay) return;

        var inventory = collector.GetComponent<Inventory>();
        if (inventory != null && itemData != null)
        {
            inventory.AddItem(new ItemStack(itemData));
            Destroy(gameObject);
        }
    }
}
