using UnityEngine;

public class GameDatabaseManager : MonoBehaviour
{
    public static GameDatabaseManager Instance { get; private set; }

    [SerializeField] private ItemDatabase itemDatabase; 
    [SerializeField] private StructureDatabase structureDatabase;
    [SerializeField] private SpriteDatabase spriteDatabase;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        itemDatabase.Initialize();
        structureDatabase.Initialize();
        spriteDatabase.Initialize();
    }

    public ItemDatabase Items => itemDatabase;
    public StructureDatabase Structures => structureDatabase;
    public  SpriteDatabase Sprites => spriteDatabase;
}
