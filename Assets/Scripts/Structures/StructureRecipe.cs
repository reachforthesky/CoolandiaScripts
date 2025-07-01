using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "StructureRecipe", menuName = "Structures/StructureRecipe")]
public class StructureRecipe : ScriptableObject
{
    public string recipeName;
    public GameObject structurePrefab;
    public GameObject structurePreviewPrefab;
    public List<ItemStack> requiredItems;
    public int toolTierRequired = 0;
    public int buildCost = 1;
    public string requiredToolType = "";
    [Tooltip("Unique ID used for network syncing")]
    public int structureId;
}
