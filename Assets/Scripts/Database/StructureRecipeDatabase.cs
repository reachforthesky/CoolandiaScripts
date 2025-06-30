using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "StructureRecipeDatabase", menuName = "Databases/StructureRecipeDatabase")]
public class StructureRecipeDatabase : ScriptableObject
{
    [SerializeField] private List<StructureRecipe> recipes = new();

    private Dictionary<int, StructureRecipe> idToRecipe = new();
    private Dictionary<StructureRecipe, int> recipeToId = new();

    public void Initialize()
    {
        idToRecipe.Clear();
        recipeToId.Clear();

        int nextId = 1;
        foreach (var recipe in recipes)
        {
            if (recipe == null) continue;

            // Assign a unique ID if it's not already assigned
            if (recipe.structureId <= 0)
                recipe.structureId = nextId;

            idToRecipe[recipe.structureId] = recipe;
            recipeToId[recipe] = recipe.structureId;

            nextId = Mathf.Max(nextId, recipe.structureId + 1);
        }
    }

    public StructureRecipe Get(int id)
    {
        idToRecipe.TryGetValue(id, out var recipe);
        return recipe;
    }

    public int GetId(StructureRecipe recipe)
    {
        recipeToId.TryGetValue(recipe, out var id);
        return id;
    }

    public List<StructureRecipe> All => recipes;
}
