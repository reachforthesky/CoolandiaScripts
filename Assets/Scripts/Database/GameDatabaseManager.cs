using System.Collections.Generic;
using System.IO;
using Unity.Collections;
using UnityEngine;

public class GameDatabaseManager : MonoBehaviour
{
    public static GameDatabaseManager Instance { get; private set; }

    [SerializeField] private string dbFileName = "GameData.db";
    public Dictionary<FixedString32Bytes, ItemData> Items { get; private set; }
    [SerializeField] private StructureDatabase structureDatabase;
    [SerializeField] private SpriteDatabase spriteDatabase;
    [SerializeField] private StructureRecipeDatabase structureRecipeDatabase;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        string dbPath = Path.Combine(Application.persistentDataPath, dbFileName);
        var allItems = ItemDataLoader.LoadAllItems(dbPath);

        Items = new Dictionary<FixedString32Bytes, ItemData>();
        foreach (var item in allItems)
        {
            Items[item.itemId] = item;
        }
        structureDatabase.Initialize();
        spriteDatabase.Initialize();
        structureRecipeDatabase.Initialize();
    }

    public StructureDatabase Structures => structureDatabase;
    public  SpriteDatabase Sprites => spriteDatabase;
    public StructureRecipeDatabase StructureRecipes => structureRecipeDatabase;
}
