// ItemData.cs
using AYellowpaper.SerializedCollections;
using NUnit.Framework;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Inventory/Item")]
public class ItemData : ScriptableObject
{
    public string itemName;
    public Sprite icon;
    public bool isStackable = true;
    public int damage = 1;
    [SerializedDictionary("Stat", "Value")]
    public SerializedDictionary<Stat, float> stats= new SerializedDictionary<Stat, float>();
    public List<Tag> tags = new List<Tag>();

    [Tooltip("Unique ID used for network syncing")]
    public int itemId;
}
