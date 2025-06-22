using UnityEngine;

public class BuildableEntityData : EntityData, IInteractable
{
    
    [SerializeField] public GameObject finishedStructurePrefab;
    [SerializeField] private GameObject buildableUIPrefab;
    public Inventory inventory;
    public StructureRecipe recipe;
    private int buildPoints = 0;

    public void Awake()
    {
        if (inventory.slots.Count > 0)
        {
            inventory.slots.Clear();
        }
        inventory.maxSlots = recipe.requiredItems.Count;
        for (int i = 0; i < inventory.maxSlots; i++)
        {
            inventory.slots.Add(new InventorySlot(ItemStack.Empty()));
        }
    }
    public bool canBuild()
    {
        foreach (var req in recipe.requiredItems)
        {
            if (!inventory.Has(req))
                return false;
        }
        return true;
    }
    public override void ItemUsed(ItemData item)
    {
        if(!item.tags.Contains(recipe.requiredToolType))
        {
            Debug.Log($"Item {item.name} is not of type {recipe.requiredToolType}");
            return;
        }
        if(!item.stats.ContainsKey(Stat.ItemTier))
        {
            Debug.Log($"Item {item.name} does not have stat ItemTier");
            return;
        }
        if (item.stats[Stat.ItemTier] < recipe.toolTierRequired)
        { 
            Debug.Log($"Item {item.name} does not have sufficient ItemTier stat");
            return;
        }
        if (!item.stats.ContainsKey(Stat.BuildPower))
        {
            Debug.Log($"Item {item.name} does not have stat BuildPower");
            return;
        }
        if (!canBuild())
        {
            Debug.Log("Missing Materials");
            return;
        }
        buildPoints += (int)item.stats[Stat.BuildPower];

        CheckIfBuilt();
    }

    public void Interact(PlayerController player)
    {
        player.OpenInventory();
        var ui = UIManager.Instance.OpenInCenter(buildableUIPrefab);
        ui.Bind(this);
    }

    private void CheckIfBuilt()
    {
        if (buildPoints >= recipe.buildCost)
        {
            // Spawn structure
            if (finishedStructurePrefab != null)
            {
                Instantiate(finishedStructurePrefab, transform.position, transform.rotation);
            }

            Destroy(gameObject);
        }
    }
}
