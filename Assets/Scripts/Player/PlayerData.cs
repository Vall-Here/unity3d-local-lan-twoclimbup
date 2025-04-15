using System;
using Unity.Collections;
using Unity.Netcode;

public struct PlayerData : INetworkSerializable, IEquatable<PlayerData>
{
    public ulong clientId;
    public FixedString128Bytes playerName;

    // Method untuk serialisasi data
    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        serializer.SerializeValue(ref clientId);
        serializer.SerializeValue(ref playerName);
    }

    // Implementasi IEquatable untuk perbandingan objek PlayerData
    public bool Equals(PlayerData other)
    {
        return clientId == other.clientId && playerName.Equals(other.playerName);
    }

    // Override Equals untuk kompatibilitas lebih luas
    public override bool Equals(object obj)
    {
        return obj is PlayerData other && Equals(other);
    }

    // Override GetHashCode untuk mendukung operasi di koleksi seperti List atau Dictionary
    public override int GetHashCode()
    {
        return clientId.GetHashCode() ^ playerName.GetHashCode();
    }
}
