using System.Collections.Generic;
using UnityEngine;

public class StructureDatabase : MonoBehaviour
{
    public static StructureDatabase Instance { get; private set; }

    [SerializeField] private List<StructureRecipe> structures = new();

    private Dictionary<int, StructureRecipe> idToStructure = new();

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

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
}
