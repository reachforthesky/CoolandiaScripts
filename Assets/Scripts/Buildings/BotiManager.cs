using System;
using System.Collections;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

public class BotiManager : MonoBehaviour
{
    public static BotiManager Instance;

    [SerializeField] private Transform interiorParent;
    [SerializeField] private Camera playerCamera;


    private int currentInteriorIndex = 0;
    private void Awake()
    {
        Instance = this;
    }

    public void EnterInterior(GameObject interior, PlayerController player)
    {

        // Move player into interior spawn point
        StartCoroutine(EnterInteriorRoutine(interior, player));

    }
    private IEnumerator EnterInteriorRoutine(GameObject interior, PlayerController player)
    {
        // Step 1: Move player first
        var spawn = interior.GetComponentInChildren<ExitBotiTrigger>();
        if (spawn)
        {
            player.Teleport(spawn.transform.position);
        }
        // Step 2: Wait one frame to ensure transform settles
        yield return null;
        // Step 3: Then safely parent the interior
        interior.GetComponent<NetworkObject>().TrySetParent(interiorParent, false);
    }
    public Vector3 GetNextInteriorSlot()
    {
        // Stack interiors vertically to avoid collisions
        Vector3 slot = new Vector3(0, -1000 * (currentInteriorIndex + 1), 0);
        currentInteriorIndex++;
        return slot;
    }
    public void ExitInterior(EntityData botiEntrance, PlayerController player)
    {
        StartCoroutine(ExitRoutine(botiEntrance, player));
    }
    private IEnumerator ExitRoutine(EntityData botiEntrance, PlayerController player)
    {
        // Step 1: Move player first
        float radius = botiEntrance.GetComponent<CapsuleCollider>()?.radius ?? 1f;
        Vector3 offset = new Vector3(radius + 1, 0, 0);
        player.Teleport(botiEntrance.transform.position + offset);

        // Step 2: Wait one frame to ensure transform settles
        yield return null;
    }
}