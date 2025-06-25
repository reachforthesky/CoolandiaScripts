using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "StructureDatabase", menuName = "Databases/StructureDatabase")]
public class StructureDatabase : ScriptableObject
{
    [SerializeField] private List<StructureRecipe> structures = new();

    private Dictionary<int, StructureRecipe> idToStructure = new();

    public void Initialize()
    {
        idToStructure.Clear();
        int i = 1;
        foreach (var structure in structures)
        {
            if (structure == null) continue;
            structure.structureId = i;
            idToStructure[i] = structure;
            i++;
        }
    }

    public StructureRecipe Get(int id)
    {
        idToStructure.TryGetValue(id, out var item);
        return item;
    }

    public List<StructureRecipe> AllStructures => structures;
}
