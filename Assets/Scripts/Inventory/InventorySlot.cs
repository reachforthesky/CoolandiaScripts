[System.Serializable]
public class InventorySlot
{
    public ItemStack stack;

    public InventorySlot(ItemStack stack)
    {
        this.stack = stack;
    }

    public bool Matches(ItemData other)
    {
        return stack.item == other;
    }
}
