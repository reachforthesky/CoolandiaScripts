using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Crafting/ProcessorRecipe")]
public class ProcessorRecipe : ScriptableObject
{
    public string recipeID;
    public List<ItemStack> inputs;
    public ItemStack output;
    public float processingCost;

    public bool Matches(ItemData item)
    {
        return inputs.Exists(stack => stack.itemId == item.itemId);
    }
}
