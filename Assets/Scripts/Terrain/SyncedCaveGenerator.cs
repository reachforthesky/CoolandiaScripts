using System.Collections;
using Unity.Netcode;
using UnityEngine;

[RequireComponent(typeof(CaveMeshGenerator))]
public class SyncedCaveGenerator : NetworkBehaviour
{
    private CaveMeshGenerator cave;
    //private int seed = -1;

    // Use NetworkVariable if needed
    private NetworkVariable<int> syncedSeed = new NetworkVariable<int>(-1);

    private void Awake()
    {
        cave = GetComponent<CaveMeshGenerator>();
    }

    public override void OnNetworkSpawn()
    {
        if (IsServer)
        {
            // Server sets the seed (already done in BotiEntrance probably)
            syncedSeed.Value = GetSeedSomehow(); // You may inject this
        }
        else if (IsClient)
        {
            StartCoroutine(WaitForSeedThenGenerate());
        }
    }

    private IEnumerator WaitForSeedThenGenerate()
    {
        yield return new WaitUntil(() => syncedSeed.Value >= 0);

        // Generate cave on the client
        cave.Generate(syncedSeed.Value);
    }

    private int GetSeedSomehow()
    {
        var entrance = GetComponentInParent<BotiEntrance>();
        return entrance != null ? entrance.id.Value : Random.Range(0, 9999); // fallback
    }
}