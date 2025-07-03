using System;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UIElements;

public class BuildableEntityData : EntityData, IInteractable
{
    public NetworkVariable<int> structureRecipeId = new();
    public NetworkVariable<FixedString32Bytes> spriteId = new();
    [SerializeField] public GameObject finishedStructurePrefab;
    [SerializeField] private GameObject buildableUIPrefab;
    public Inventory inventory;
    public StructureRecipe recipe;
    private int buildPoints = 0;
    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        structureRecipeId.OnValueChanged += UpdateRecipe;

        // Resolve structure recipe on client
        if (IsClient)
        {
            recipe = GameDatabaseManager.Instance.Structures.Get(structureRecipeId.Value);
        }

        if (inventory.syncedSlots.Count == 0)
        {
            inventory.maxSlots = recipe.requiredItems.Count;
            for (int i = 0; i < inventory.maxSlots; i++)
                inventory.syncedSlots.Add(ItemStack.Empty());
        }

        // Sync sprite if already assigned
        var sprite = GameDatabaseManager.Instance.Sprites[spriteId.Value];
        if (sprite != null)
        {
            GetComponent<SpriteRenderer>().sprite = sprite;
        }
    }
    private void InitializeInventory()
    {
        if (recipe == null)
        {
            Debug.LogWarning("Tried to init inventory before recipe was set.");
            return;
        }

        inventory.maxSlots = recipe.requiredItems.Count;
        inventory.syncedSlots.Clear();
        for (int i = 0; i < inventory.maxSlots; i++)
        {
            inventory.syncedSlots.Add(ItemStack.Empty());
        }
    }
    private void UpdateRecipe(int previousValue, int newValue)
    {
        recipe = GameDatabaseManager.Instance.Structures.Get(newValue);
        InitializeInventory();
    }

    /*public void Awake()
{
   if (inventory.syncedSlots.Count > 0)
   {
       inventory.syncedSlots.Clear();
   }
   inventory.maxSlots = recipe.requiredItems.Count;
   for (int i = 0; i < inventory.maxSlots; i++)
   {
       inventory.syncedSlots.Add(ItemStack.Empty());
   }
}*/

    private void OnEnable()
    {
        spriteId.OnValueChanged += OnSpriteIdChanged;
    }

    private void OnDisable()
    {
        spriteId.OnValueChanged -= OnSpriteIdChanged;
    }
    private void OnSpriteIdChanged(FixedString32Bytes oldVal, FixedString32Bytes newVal)
    {
        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        Sprite sprite = GameDatabaseManager.Instance.Sprites[newVal];
        if (sprite != null)
            sr.sprite = sprite;
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
            Debug.Log($"Item {item.itemName} is not of type {recipe.requiredToolType}");
            return;
        }
        if(!item.stats.ContainsKey("ItemTier"))
        {
            Debug.Log($"Item {item.itemName} does not have stat ItemTier");
            return;
        }
        if (item.stats["ItemTier"] < recipe.toolTierRequired)
        { 
            Debug.Log($"Item {item.itemName} does not have sufficient ItemTier stat");
            return;
        }
        if (!item.stats.ContainsKey("BuildPower"))
        {
            Debug.Log($"Item {item.itemName} does not have stat BuildPower");
            return;
        }
        if (!canBuild())
        {
            Debug.Log("Missing Materials");
            return;
        }
        buildPoints += (int)item.stats["BuildPower"];

        CheckIfBuilt();
    }

    new public void Interact(PlayerController player)
    {
        player.OpenInventory();
        RequestInventoryDataServerRpc(player.OwnerClientId);
        //var ui = UIManager.LocalInstance.OpenInCenter(buildableUIPrefab);
        //ui.Bind(this);
    }

    private void CheckIfBuilt()
    {
        if (buildPoints >= recipe.buildCost)
        {
            // Spawn structure
            if (finishedStructurePrefab != null)
            {
                int prefabId = Array.IndexOf(PersistentEntityManager.Instance.entityPrefabs, finishedStructurePrefab);
                var buildablePed = new PersistentEntityData
                {
                    prefabId = prefabId,
                    position = transform.position,
                    rotation = Quaternion.identity, // Default rotation
                    isDestroyed = false,
                };
                PersistentEntityManager.Instance.RegisterEntity(buildablePed);
            }

            Destroy(gameObject);
        }
    }
    [ServerRpc(RequireOwnership = false)]
    public void RequestInventoryDataServerRpc(ulong clientId)
    {

        List<FixedString32Bytes> itemIds = new();
        List<int> quantities = new();

        foreach (var slot in inventory.syncedSlots)
        {
            var stack = slot;
            itemIds.Add(stack.itemId);
            quantities.Add(stack.quantity);
        }

        SendInventoryDataClientRpc(itemIds.ToArray(), quantities.ToArray(), new ClientRpcParams
        {
            Send = new ClientRpcSendParams { TargetClientIds = new[] { clientId } }
        });
    }
    [ClientRpc]
    private void SendInventoryDataClientRpc(FixedString32Bytes[] itemIds, int[] quantities, ClientRpcParams rpcParams = default)
    {
        if (itemIds.Length != quantities.Length)
        {
            Debug.LogError("Inventory data mismatch!");
            return;
        }

        // Create a fresh copy for local UI
        var clientInventory = inventory;
        inventory.syncedSlots.Clear();
        for (int i = 0; i < clientInventory.maxSlots; i++)
        {
            //var item = GameDatabaseManager.Instance.Items.Get(itemIds[i]);
            clientInventory.syncedSlots.Add(new ItemStack(itemIds[i], quantities[i]));
        }

        // Open the UI with the synced inventory
        var ui = UIManager.LocalInstance.OpenInCenter(buildableUIPrefab);
        this.inventory = clientInventory;
        ui.Bind(this);
    }
}
