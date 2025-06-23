using Unity.Netcode;
using UnityEngine;

public class BuildingManager : NetworkBehaviour
{

    public static BuildingManager Instance { get; private set; }
    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    
}
