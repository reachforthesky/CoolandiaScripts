using UnityEngine;
using UnityEngine.UIElements;

public class CampfireUI : MonoBehaviour, IBindableUI
{
    [SerializeField] private InventoryUI fuelUI;
    [SerializeField] private InventoryUI cookingUI;

    private CampfireController controller;
    public void Bind(object campfire)
    {
        if(campfire.GetType() != typeof(CampfireController))
        {
            Debug.LogError("CampfireUI can only bind to CampfireController instances.");
            return;
        }
        controller = (CampfireController)campfire;
        fuelUI.Bind(controller.GetFuelInventory());
        cookingUI.Bind(controller.GetProcessingInventory());
    }

    public void Activate()
    {
        controller.active = !controller.active;
    }

    public void Close()
    {
        Destroy(gameObject);
    }
}
