[System.Serializable]
public class InventorySlot
{
    public ItemStack stack;

    public InventorySlot(ItemStack stack)
    {
        this.stack = stack;
    }

    public bool Matches(string otherId)
    {
        return stack.itemId == otherId;
    }
}
