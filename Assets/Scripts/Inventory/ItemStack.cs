using System;

[System.Serializable]
public class ItemStack
{
    public ItemData item;
    public int quantity;

    public ItemStack(ItemData item, int quantity = 1)
    {
        this.item = item;
        this.quantity = quantity;
    }

    public void ReduceBy(int amount)
    {
        this.quantity -= amount;
        if(this.quantity <= 0)
        {
            this.quantity = 0;
            this.item = null;
        }

    }

    public ItemStack Clone()
    {
        return new ItemStack(this.item, this.quantity);
    }

    public static ItemStack Empty()
    {
        return new ItemStack(null, 0);
    }
}
