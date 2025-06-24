using System.Collections;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UIElements;

public class BotiEntrance : NetworkBehaviour
{
    [SerializeField] private EntityData botiEntranceData;
    [SerializeField] private GameObject interiorPrefab;

    private GameObject activeInterior;
    private ExitBotiTrigger exitTrigger;
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

        if (activeInterior == null)
        {
            StartCoroutine(SpawnInteriorCoroutine());
        }
    }

    public void Interact(PlayerController player)
    {
        if (!exitTrigger)
        {
            exitTrigger = activeInterior.GetComponentInChildren<ExitBotiTrigger>();
            exitTrigger.setBotiEntrance(botiEntranceData);
        }
        if(exitTrigger)
            BotiManager.Instance.EnterInterior(activeInterior, player);
    }

    private IEnumerator SpawnInteriorCoroutine()
    {
        // Wait until PEM is ready
        yield return new WaitUntil(() => PersistentEntityManager.Instance != null && PersistentEntityManager.Instance.IsSpawned);

        var prefabIndex = PersistentEntityManager.Instance.FindIndex(interiorPrefab);
        var data = new PersistentEntityData
        {
            prefabId = prefabIndex,
            position = BotiManager.Instance.GetNextInteriorSlot(),
            rotation = Quaternion.identity,
            isDestroyed = false
        };
        var botiParent = BotiManager.Instance.GetComponentInChildren<NetworkObject>().transform;
        activeInterior = PersistentEntityManager.Instance.RegisterEntity(data, botiParent);
        var exitSpawn = activeInterior.GetComponent<ExitBotiTriggerSpawn>();
        StartCoroutine(exitSpawn.SpawnExitCoroutine(trigger =>
        {
            Debug.Log("Exit trigger spawned: " + trigger.name);
        }));
        activeInterior.name = $"Interior_{gameObject.name}";
        var terrainGenerator = activeInterior.GetComponent<ITerrainGenerator>();
        if (terrainGenerator != null)
            if (id != 0)
                terrainGenerator.Generate(id);
            else
                terrainGenerator.Generate();
        activeInterior.transform.position = BotiManager.Instance.GetNextInteriorSlot();
    }
}

