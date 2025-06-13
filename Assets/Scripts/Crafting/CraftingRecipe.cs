using UnityEngine;

[CreateAssetMenu(menuName = "Crafting/Recipe")]
public class CraftingRecipe : ScriptableObject
{
    public ItemStack[] ingredients;
    public ItemStack output;
    public string recipeName;
}
