using AYellowpaper.SerializedCollections;
using UnityEngine;

public class StatHandler : MonoBehaviour
{

    [SerializedDictionary("Stat", "Value")]
    public SerializedDictionary<Stat, int> stats = new SerializedDictionary<Stat, int>();

    private PlayerEquipment equipment;

    private void Awake()
    {
        equipment = GetComponent<PlayerEquipment>();
    }
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public int GetEffectiveStat(Stat stat)
    {
        int baseStat = stats[stat];

        if(equipment != null)
        {
            foreach (var equip in equipment.equippedArmor)
            {
                baseStat += equip.statBonuses[stat];
            }
        }

        return baseStat;
    }
}
