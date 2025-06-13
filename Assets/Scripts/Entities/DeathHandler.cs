using UnityEngine;

public class DeathHandler : MonoBehaviour
{
    public GameObject parentObject;
    private StatHandler statHandler;
    private EntityData parentEntityData;


    private void Awake()
    {
        statHandler = GetComponent<StatHandler>();
        parentEntityData = GetComponent<EntityData>();
        parentEntityData.receiveHit += CheckForDeath;
    }
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private void CheckForDeath(int damageTaken)
    {
        if (statHandler && statHandler.stats.ContainsKey(Stat.health) && statHandler.stats[Stat.health] <= 0)
        {
            parentEntityData.DestroyEntity();
        }
    }
}
