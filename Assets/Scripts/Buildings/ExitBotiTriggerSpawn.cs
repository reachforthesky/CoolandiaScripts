using System.Collections;
using Unity.Netcode;
using UnityEngine;

public class ExitBotiTriggerSpawn : MonoBehaviour
{
    [SerializeField] private Vector3 position;
    [SerializeField] private GameObject exitBotiTriggerPrefab;
    private int prefabIndex;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        prefabIndex = PersistentEntityManager.Instance.FindIndex(exitBotiTriggerPrefab);
    }

    public IEnumerator SpawnExitCoroutine(System.Action<GameObject> onSpawned)
    {
        // Wait until PEM is ready
        yield return new WaitUntil(() => PersistentEntityManager.Instance != null && PersistentEntityManager.Instance.IsSpawned);

        var data = new PersistentEntityData
        {
            prefabId = prefabIndex,
            position = transform.position + position,
            rotation = Quaternion.identity,
            isDestroyed = false
        };

        var exitTrigger = PersistentEntityManager.Instance.RegisterEntity(data, transform);

        if (exitTrigger != null)
            exitTrigger.GetComponent<NetworkObject>().TrySetParent(transform);

        // Return via callback
        onSpawned?.Invoke(exitTrigger);
    }
}
