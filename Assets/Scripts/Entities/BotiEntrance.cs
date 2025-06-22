using UnityEngine;

public class BotiEntrance : MonoBehaviour
{
    [SerializeField] private EntityData botiEntranceData;
    [SerializeField] private GameObject interiorPrefab;

    private GameObject activeInterior;
    public int id = 0;

    private void Start()
    {
        if (interiorPrefab == null)
        {
            Debug.LogError("Interior prefab is not assigned in BotiEntrance.");
        }
        if (botiEntranceData == null)
        {
            Debug.LogError("Boti entrance data is not assigned in BotiEntrance.");
        }

        botiEntranceData.interacted += Interact;
    }

    public void Interact(PlayerController player)
    {
        if (activeInterior == null)
        {
            activeInterior = Instantiate(interiorPrefab);
            activeInterior.name = $"Interior_{gameObject.name}";
            var terrainGenerator = activeInterior.GetComponent<ITerrainGenerator>();
            if (terrainGenerator != null)
                if (id != 0)
                    terrainGenerator.Generate(id);
                else
                    terrainGenerator.Generate(); 
            activeInterior.transform.position = BotiManager.Instance.GetNextInteriorSlot();
        }
        var activeInteriorExit = activeInterior.GetComponentInChildren<ExitBotiTrigger>();
        //interiorSpawnPoint = activeInteriorExit.transform;
        activeInteriorExit.setBotiEntrance(botiEntranceData);
        BotiManager.Instance.EnterInterior(activeInterior, player);
    }
}
