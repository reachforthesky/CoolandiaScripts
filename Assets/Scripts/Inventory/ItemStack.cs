using System;
using Unity.Netcode;

[System.Serializable]
public struct ItemStack : INetworkSerializable, IEquatable<ItemStack>
{
    public int itemId;
    public int quantity;

    public ItemStack(int itemId, int quantity = 1)
    {
        this.itemId = itemId;
        this.quantity = quantity;
    }

    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        serializer.SerializeValue(ref itemId);
        serializer.SerializeValue(ref quantity);
    }

    public void ReduceBy(int amount)
    {
        this.quantity -= amount;
        if(this.quantity <= 0)
        {
            this.quantity = 0;
            this.itemId = 0;
        }

    }

    public bool Equals(ItemStack otherStack)
    {
        return (this.itemId == otherStack.itemId && 
            this.quantity == otherStack.quantity);
    }

    public ItemStack Clone()
    {
        return new ItemStack(this.itemId, this.quantity);
    }

    public static ItemStack Empty()
    {
        return new ItemStack(0, 0);
    }

    public bool IsEmpty() { return this.itemId == 0 || this.quantity == 0; }
}
