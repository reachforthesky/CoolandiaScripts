using AYellowpaper.SerializedCollections;
using UnityEngine;

public class StatHandler : MonoBehaviour
{

    [SerializedDictionary("Stat", "Value")]
    public SerializedDictionary<string, float> stats = new SerializedDictionary<string, float>();

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

    public float GetEffectiveStat(string stat)
    {
        var baseStat = stats[stat];

        if(equipment != null)
        {
            foreach (var equip in equipment.equippedArmor)
            {
                baseStat += equip.stats[stat];
            }
        }

        return baseStat;
    }
}
