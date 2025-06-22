#nullable enable

using System;
using System.Collections.Generic;
using UnityEngine;

public class PlayerEquipment : MonoBehaviour
{
    public ItemData?[] equippedArmor = new ItemData[6];

    public event Action? onPlayerEquipNew;

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Keypad3))
        {
            ItemData temp = (ItemData)ScriptableObject.CreateInstance("ItemData");
            temp.isStackable = false;
            temp.tags = new List<Tag> { Tag.Belt };
            temp.itemName = "toolbelt";
            temp.stats.Add(Stat.ToolbeltSize, 3);
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

    public ItemData? EquipArmor(ItemData armor)
    {
        int armorIndex = ItemDataToIndex(armor);
        if (armorIndex < 0)
            throw new Exception("Equipped Armor does not contain proper tag");
        ItemData? replacedArmor = equippedArmor[armorIndex];
        equippedArmor[armorIndex] = armor;
        onPlayerEquipNew?.Invoke();
        return replacedArmor;
    }

    private int ItemDataToIndex(ItemData item)
    {
        foreach (Tag tag in item.tags) {
            switch (tag)
            {
                case Tag.Belt:
                    return 2;
                default:
                    continue;
            }
        }
        return -1;
    }
}