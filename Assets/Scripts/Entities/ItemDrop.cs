using Unity.Collections;
using Unity.Netcode;
using UnityEngine;

public class ItemDrop : NetworkBehaviour
{
    [SerializeField] private float attractionRadius = 3f;
    [SerializeField] private float attractionSpeed = 5f;
    [SerializeField] private float maxSpeed = 10f;

    public SpriteRenderer iconRenderer;
    public float lifetime = 30f;
    public float targetSize = 1f;
    public float verticleOffset = .05f;
    public float collectDelay = 0.3f;

    private Rigidbody rb;
    private Transform playerTransform;
    private float spawnTime;
    private bool pickedUp = false;

    private ItemData itemData;

    public NetworkVariable<FixedString32Bytes> itemId = new("", NetworkVariableReadPermission.Everyone);

    public override void OnNetworkSpawn()
    {
        itemId.OnValueChanged += OnItemIdChanged;

        if (IsServer)
        {
            itemId.Value = itemData.itemId;
        }

        if (IsClient)
        {
            TryResolveItem(itemId.Value);
        }
    }

    private void OnItemIdChanged(FixedString32Bytes previous, FixedString32Bytes current)
    {
        TryResolveItem(current);
    }

    private void TryResolveItem(FixedString32Bytes id)
    {
        itemData = GameDatabaseManager.Instance.Items[id];
        UpdateVisuals();
    }

    private void UpdateVisuals()
    {
        if (iconRenderer != null && itemData.iconId != null)
        {
            var icon = GameDatabaseManager.Instance.Sprites[itemData.iconId];
            iconRenderer.sprite = icon;

            float spriteHeight = icon.bounds.size.y;
            float scale = targetSize / spriteHeight;
            iconRenderer.transform.localScale = Vector3.one * scale;
            iconRenderer.transform.localPosition = new Vector3(0f, verticleOffset, 0f);
        }
    }

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        spawnTime = Time.time;

        if (!IsServer)
        {
            rb.isKinematic = true;
        }

        // Find local player for magnetic attraction
        foreach (var player in FindObjectsByType<PlayerController>(FindObjectsSortMode.None))
        {
            if (player.IsOwner)
            {
                playerTransform = player.transform;
                break;
            }
        }

        if (IsServer)
        {
            Destroy(gameObject, lifetime);
        }
    }

    public void Initialize(ItemData item)
    {
        itemData = item;
        if (IsServer)
        {
            itemId.Value = item.itemId;
        }

        UpdateVisuals();

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

    private void FixedUpdate()
    {
        if (playerTransform == null || rb == null || Time.time - spawnTime < collectDelay)
            return;

        Vector3 toPlayer = playerTransform.position - transform.position;
        float distance = toPlayer.magnitude;

        if (distance <= attractionRadius)
        {
            Vector3 forceDir = toPlayer.normalized;
            float forceMagnitude = Mathf.Lerp(0f, attractionSpeed, 1f - (distance / attractionRadius));
            Vector3 force = forceDir * forceMagnitude;

            if (rb.linearVelocity.magnitude < maxSpeed)
            {
                rb.AddForce(force, ForceMode.Acceleration);
            }
        }
    }

    public void TryPickup(GameObject collector)
    {
        if (!IsServer || pickedUp || !itemData.IsEmpty() ) return;

        var inventory = collector.GetComponent<Inventory>();
        if (inventory)
        {
            inventory.AddItem(new ItemStack(itemData.itemId));
            pickedUp = true;
            NetworkObject.Despawn(); // Properly sync across clients
        }
    }
    public override void OnDestroy()
    {
        itemId.OnValueChanged -= OnItemIdChanged;
        base.OnDestroy();
    }
}