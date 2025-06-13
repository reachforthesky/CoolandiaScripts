using TMPro;
using UnityEngine;
using UnityEngine.UI;


public class CraftingUI : MonoBehaviour
{
    [SerializeField] private CraftingSystem craftingSystem;
    [SerializeField] private CraftingRecipe[] availableRecipes;
    [SerializeField] private GameObject recipeButtonPrefab;
    [SerializeField] private Transform recipeListParent;

    void Start()
    {
        foreach (var recipe in availableRecipes)
        {
            var r = recipe;
            var buttonGO = Instantiate(recipeButtonPrefab, recipeListParent);
            var btn = buttonGO.GetComponent<Button>();
            var text = buttonGO.GetComponentInChildren<TMP_Text>();
            text.text = recipe.recipeName;

            btn.onClick.AddListener(() => craftingSystem.Craft(r));
        }
    }
}
