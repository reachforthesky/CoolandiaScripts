using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UIElements;

public class PersistentEntityManager : NetworkBehaviour
{
    public static PersistentEntityManager Instance { get; private set; }

    public NetworkList<PersistentEntityData> entities;

    public GameObject[] entityPrefabs; // Assign your spawnable prefabs in inspector

    private Dictionary<int, NetworkObject> runtimeSpawnedEntities = new();

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
    }
    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        entities = new NetworkList<PersistentEntityData>();
    }

    public GameObject RegisterEntity(PersistentEntityData data)
    {
        //if (!IsServer) return null;
        if (!IsSpawned)
        {
            Debug.Log("Persistent Entity Manager is not yet spawned");
            return null;
        }
        int index = entities.Count;
        entities.Add(data); 

        var prefab = entityPrefabs[data.prefabId];
        var go = Instantiate(prefab, data.position, data.rotation, transform);
        var netObj = go.GetComponent<NetworkObject>();
        netObj.Spawn();

        runtimeSpawnedEntities.Add(index, netObj);

        var entityData = go.GetComponent<EntityData>();
        entityData.persistentIndex = index;
        return go;
    }

    public void MarkDestroyed(int index)
    {
        if (!IsServer) return;

        var entry = entities[index];
        entry.isDestroyed = true;
        entities[index] = entry;

        if (runtimeSpawnedEntities.TryGetValue(index, out var netObj))
        {
            netObj.Despawn();
            runtimeSpawnedEntities.Remove(index);
        }
    }

    private void OnServerEntityListChanged(NetworkListEvent<PersistentEntityData> changeEvent)
    {
        if (changeEvent.Type != NetworkListEvent<PersistentEntityData>.EventType.Add)
            return;

        var data = changeEvent.Value;
        if (data.isDestroyed) return;

        var prefab = entityPrefabs[data.prefabId];
        var go = Instantiate(prefab, data.position, data.rotation);
        var netObj = go.GetComponent<NetworkObject>();
        netObj.Spawn();

        int index = changeEvent.Index;
        runtimeSpawnedEntities.Add(index, netObj);

        var entityData = go.GetComponent<EntityData>();
        entityData.persistentIndex = index;
    }
    private void OnClientEntityListChanged(NetworkListEvent<PersistentEntityData> changeEvent)
    {
        if (changeEvent.Type == NetworkListEvent<PersistentEntityData>.EventType.Value)
        {
            var data = changeEvent.Value;
            int index = changeEvent.Index;

            if (data.isDestroyed)
            {
                // Client should destroy the local entity instance
                if (runtimeSpawnedEntities.TryGetValue(index, out var obj))
                {
                    Destroy(obj.gameObject);
                    runtimeSpawnedEntities.Remove(index);
                }
            }
        }
    }
}
