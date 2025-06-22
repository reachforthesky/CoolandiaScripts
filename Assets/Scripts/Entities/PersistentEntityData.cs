using System;
using Unity.Netcode;
using UnityEngine;

[System.Serializable]
public struct PersistentEntityData : INetworkSerializable, IEquatable<PersistentEntityData>
{
    public int prefabId; // Index into your prefab array
    public Vector3 position;
    public Quaternion rotation;
    public bool isDestroyed;

    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        serializer.SerializeValue(ref prefabId);
        serializer.SerializeValue(ref position);
        serializer.SerializeValue(ref rotation);
        serializer.SerializeValue(ref isDestroyed);
    }

    public bool Equals(PersistentEntityData other)
    {
        return prefabId == other.prefabId &&
               position == other.position &&
               rotation == other.rotation &&
               isDestroyed == other.isDestroyed;
    }

    public override bool Equals(object obj)
    {
        return obj is PersistentEntityData other && Equals(other);
    }

    public override int GetHashCode()
    {
        return prefabId.GetHashCode() ^
               position.GetHashCode() ^
               rotation.GetHashCode() ^
               isDestroyed.GetHashCode();
    }
}
