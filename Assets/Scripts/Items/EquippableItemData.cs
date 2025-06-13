// ItemData.cs
using AYellowpaper.SerializedCollections;
using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Inventory/Equippable Item")]
public class EquippableItemData : ItemData
{
    [SerializedDictionary("Stat", "Value")]
    public SerializedDictionary<Stat, int> statBonuses = new SerializedDictionary<Stat, int>();
    public EquipType equipType;
}

public enum EquipType { Head, Back, Belt, Gloves, Pants, Shoes }