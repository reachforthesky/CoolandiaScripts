using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class CampfireController : MonoBehaviour, IInteractable
{
    public Sprite inactiveSprite;
    public Sprite activeSprite;

    [SerializeField] private FuelConsumer fuelConsumer;
    [SerializeField] private Processor processor;

    [Header("UI")]
    [SerializeField] private GameObject campfireUIPrefab;

    private SpriteRenderer spriteRenderer;

    public bool active;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();

    }

    private void Update()
    {
        if (active)
        {
            fuelConsumer.enabled = true;
            if (fuelConsumer.IsBurning)
                spriteRenderer.sprite = activeSprite;
            else
                spriteRenderer.sprite = inactiveSprite;

            processor.enabled = fuelConsumer.IsBurning;
        }
        else
        {
            spriteRenderer.sprite = inactiveSprite;
            fuelConsumer.enabled = processor.enabled = false;
        }
    }

    public void Interact(PlayerController player)
    {
        var canvas = GameObject.FindWithTag("MainCanvas");
        if (!canvas)
        {
            Debug.LogError("MainCanvas not found!");
            return;
        }

        player.OpenInventory();
        var ui = UIManager.Instance.OpenInCenter(campfireUIPrefab);
        ui?.Bind(this);
    }

    public Inventory GetFuelInventory() => fuelConsumer.fuelInventory;
    public Inventory GetProcessingInventory() => processor.inventory;
}
