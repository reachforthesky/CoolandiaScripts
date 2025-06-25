using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "SpriteDatabase", menuName = "Databases/SpriteDatabase")]
public class SpriteDatabase : ScriptableObject
{
    [SerializeField] private List<SpriteEntry> sprites = new();

    private Dictionary<string, Sprite> idToSprite = new(); 
    private Dictionary<Sprite, string> spriteToId = new();

    public void Initialize()
    {
        idToSprite.Clear();
        spriteToId.Clear();
        foreach (var entry in sprites)
        {
            if (entry.sprite == null || string.IsNullOrEmpty(entry.id)) continue;
            idToSprite[entry.id] = entry.sprite;
            spriteToId[entry.sprite] = entry.id;
        }
    }

    public Sprite GetSprite(string id)
    {
        idToSprite.TryGetValue(id, out var sprite);
        return sprite;
    }
    public string GetId(Sprite sprite)
    {
        spriteToId.TryGetValue(sprite, out var id);
        return id;
    }
    public List<SpriteEntry> AllSprites => sprites;
}

[Serializable]
public struct SpriteEntry
{
    public string id;       // e.g., "axe_icon", "campfire", etc.
    public Sprite sprite;
}
