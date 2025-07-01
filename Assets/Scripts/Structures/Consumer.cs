using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class FuelConsumer : MonoBehaviour
{
    [SerializeField] public Inventory fuelInventory;
    public float defaultBurnTime = 5f; // Fallback
    public List<ItemData> acceptedFuels;

    private float fuelTimer;
    public bool IsBurning => fuelTimer > 0;

    private void Awake()
    {
        if (!fuelInventory)
            fuelInventory = GetComponent<Inventory>();
    }
    private void Update()
    {
        if (!IsBurning) {
            ItemData selectedFuel = ItemData.Empty();
            foreach (var slot in fuelInventory.syncedSlots)
            {
                if (acceptedFuels.Where<ItemData>(item => item.itemId == slot.itemId).Count() > 0)
                    selectedFuel = GameDatabaseManager.Instance.Items[slot.itemId];
            }
            if (!selectedFuel.IsEmpty())
                fuelInventory.RemoveStack(new ItemStack(selectedFuel.itemId, 1));
                AddFuel(selectedFuel, 1);
        }
        
        TickFuel(Time.deltaTime);
    }
    public void AddFuel(ItemData fuelItem, int count)
    {
        if (!acceptedFuels.Contains(fuelItem)) return;

        float fuelValue = GetFuelValue(fuelItem);
        fuelTimer += fuelValue * count;
    }

    private float GetFuelValue(ItemData item)
    {
        if (!item.IsEmpty() && item.stats != null && item.stats.TryGetValue("FuelAmount", out float value))
        {
            return value;
        }
        return defaultBurnTime;
    }

    public void TickFuel(float deltaTime)
    {
        if (fuelTimer > 0f)
            fuelTimer -= deltaTime;
    }
}
