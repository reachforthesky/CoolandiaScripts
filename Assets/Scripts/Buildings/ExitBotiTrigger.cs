using Unity.Netcode;
using UnityEngine;

public class ExitBotiTrigger : NetworkBehaviour
{
    [SerializeField] private EntityData botiExitData;
    private EntityData botiEntranceData;

    public override void OnNetworkSpawn()
    {
        if (!IsServer) return;
        if (botiExitData == null)
        {
            Debug.LogError("Boti exit data is not assigned in ExitBotiTrigger.");
            return;
        }
        botiExitData.interacted += Interact;
    }

    private void Interact(PlayerController player)
    {
        if (botiEntranceData == null)
        {
            Debug.LogError("Boti entrance data is not assigned in ExitBotiTrigger.");
            return;
        }
        // Logic to handle exiting the Boti
        Debug.Log($"{player.name} is exiting the Boti.");

        BotiManager.Instance.ExitInterior(botiEntranceData, player);
    }
    public void setBotiEntrance(EntityData entranceData)
    {
        botiEntranceData = entranceData;
    }
}