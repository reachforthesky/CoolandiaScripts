#nullable enable

using System;
using UnityEngine;

public class PlayerEquipment : MonoBehaviour
{
    public EquippableItemData?[] equippedArmor = new EquippableItemData[6];

    public event Action? onPlayerEquipNew;

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Keypad3))
        {
            EquippableItemData temp = (EquippableItemData)ScriptableObject.CreateInstance("EquippableItemData");
            temp.isStackable = false;
            temp.equipType = EquipType.Belt;
            temp.toolType = ToolType.None;
            temp.itemName = "toolbelt";
            temp.statBonuses.Add(Stat.toolbeltSize, 3);
            EquipArmor(temp);
        }
    }

    private void Start()
    {
        for (int i = 0; i < 6; i++)
        {
            equippedArmor[i] = null;
        }
    }

    public EquippableItemData? EquipArmor(EquippableItemData armor)
    {
        int armorIndex = (int)armor.equipType;
        EquippableItemData? replacedArmor = equippedArmor[armorIndex];
        equippedArmor[armorIndex] = armor;
        onPlayerEquipNew?.Invoke();
        return replacedArmor;
    }
}