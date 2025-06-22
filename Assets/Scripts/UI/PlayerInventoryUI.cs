using UnityEngine;

public class PlayerInventoryUI : MonoBehaviour
{
    [SerializeField] private InventoryUI playerInventoryUI;
    [SerializeField] private Inventory playerInventory;
    void Start()
    {
        playerInventoryUI.Bind(playerInventory);
    }
}
