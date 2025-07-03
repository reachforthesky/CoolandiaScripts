using System;
using System.Collections.Generic;
using System.IO;
using Unity.Collections;
using UnityEngine;

public class GameDatabaseManager : MonoBehaviour
{
    public static GameDatabaseManager Instance { get; private set; }

    [SerializeField] private string dbFileName = "GameData.db";
    public Dictionary<FixedString32Bytes, ItemData> Items { get; private set; }
    public Dictionary<FixedString32Bytes, Sprite> Sprites { get; private set; }
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

        string dbPath = GetDbPath(dbFileName);
        CreateItemDict(dbPath);
        CreateSpriteDict(dbPath);
        structureDatabase.Initialize();
        spriteDatabase.Initialize();
        structureRecipeDatabase.Initialize();
    }
    private string GetDbPath(string dbFileName)
    {
        string sourcePath = Path.Combine(Application.streamingAssetsPath, dbFileName);
        //string sourcePath = "C:\\Users\\Brian\\source\\repos\\OutsetDBManager\\OutsetDBManager.Server\\GameData.db";
        string destPath = Path.Combine(Application.persistentDataPath, dbFileName);

        // Convert Windows backslashes to forward slashes for URI
        destPath = destPath.Replace("\\", "/");

        // Actually copy the file if needed:
        if (!System.IO.File.Exists(destPath))
        {
            Debug.Log($"[DB] Copying DB from {sourcePath} to {destPath}"); 
            System.IO.File.Copy(sourcePath, destPath, true);
            System.Threading.Thread.Sleep(50); // 50ms is enough
        }
        else
        {
            Debug.Log($"[DB] Found DB at: {destPath}");
        }

        return destPath;
    }
    private void CreateItemDict(string dbPath)
    {
        var allItems = ItemDataLoader.LoadAllItems(dbPath);

        Items = new Dictionary<FixedString32Bytes, ItemData>();
        foreach (var item in allItems)
        {
            Items[item.itemId] = item;
        }
    }
    private void CreateSpriteDict(string dbPath)
    {
        var allSprites = SpriteDataLoader.LoadAllSprites(dbPath);

        Sprites = new Dictionary<FixedString32Bytes, Sprite>();

        foreach (var spriteData in allSprites)
        {
            var truePath = spriteData.FilePath.Replace("Assets/", "");

            if (string.IsNullOrEmpty(spriteData.SpriteId) || string.IsNullOrEmpty(spriteData.FilePath))
            {
                Debug.LogWarning($"Sprite data missing: {spriteData.SpriteId}");
                continue;
            }

            if(spriteData.Frame == null)
            {
                var sprite = Resources.Load<Sprite>(truePath);
                Sprites[spriteData.SpriteId] = sprite;
                continue;
            }
            // Load ALL sliced frames for the sheet
            var sheetSprites = Resources.LoadAll<Sprite>(spriteData.FilePath);

            if (sheetSprites == null || sheetSprites.Length == 0)
            {
                Debug.LogWarning($"No sprites found for path: {spriteData.FilePath}");
                continue;
            }

            if (spriteData.Frame < 0 || spriteData.Frame >= sheetSprites.Length)
            {
                Debug.LogWarning($"Frame {spriteData.Frame} is out of bounds for {spriteData.FilePath} (has {sheetSprites.Length} slices).");
                continue;
            }

            var spriteSlice = sheetSprites[(int)spriteData.Frame];

            Sprites[spriteData.SpriteId] = spriteSlice;
        }
    }


    public StructureDatabase Structures => structureDatabase;
    public StructureRecipeDatabase StructureRecipes => structureRecipeDatabase;
}
