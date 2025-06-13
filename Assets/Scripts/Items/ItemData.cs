// ItemData.cs
using UnityEngine;

[CreateAssetMenu(menuName = "Inventory/Item")]
public class ItemData : ScriptableObject
{
    public string itemName;
    public Sprite icon;
    public bool isStackable = true;
    public ToolType toolType;
}

public enum ToolType { None, Axe, Pick }