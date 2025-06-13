using UnityEngine;

public class CraftingSystem : MonoBehaviour
{
    [SerializeField] private Inventory playerInventory;

    public bool CanCraft(CraftingRecipe recipe)
    {
        foreach (var req in recipe.ingredients)
        {
            if (!playerInventory.Has(req))
                return false;
        }
        return true;
    }

    public bool Craft(CraftingRecipe recipe)
    {
        if (!CanCraft(recipe))
            return false;

        // Consume items
        foreach (var req in recipe.ingredients)
        {
            playerInventory.RemoveStack(req);
        }

        // Add result
        playerInventory.AddItem(recipe.output);
        return true;
    }
}
