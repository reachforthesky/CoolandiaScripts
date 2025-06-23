using UnityEngine;

public interface IInteractable
{
    ulong NetworkObjectId { get; set; }

    public void Interact(PlayerController player);

    public ulong GetNetworkObjectId()
    {
        return NetworkObjectId;
    }
}
