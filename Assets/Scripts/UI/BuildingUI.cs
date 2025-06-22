using TMPro;
using UnityEngine;

public class BuildingUI : MonoBehaviour, IBindableUI
{
    [SerializeField] private InventoryUI inventoryUI;
    [SerializeField] private TextMeshProUGUI textbox;

    private BuildableEntityData entity;
    public void Bind(object building)
    {
        if (building.GetType() != typeof(BuildableEntityData))
        {
            Debug.LogError("BuildingUI can only bind to BuildableEntityData instances.");
            return;
        }
        entity = (BuildableEntityData)building;
        inventoryUI.Bind(entity.inventory);
        string recipeString = "Materials required:";
        foreach (var stack in entity.recipe.requiredItems)
        {
            recipeString += $" {stack.item.name}x{stack.quantity}";
        }
        textbox.text = recipeString;
    }

    public void Close()
    {
        Destroy(gameObject);
    }
}
