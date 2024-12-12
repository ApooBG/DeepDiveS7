using Unity.Collections;
using Unity.Netcode;

public struct NetworkString : INetworkSerializable
{
    /*
        Copied from Unity Docs. this script allows to store a string in a network variable and get it.
    */
    private FixedString32Bytes info;
    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        serializer.SerializeValue(ref info);
    }

    public override string ToString()
    {
        return info.ToString();
    }

    public static implicit operator string(NetworkString s) => s.ToString();
    public static implicit operator NetworkString(string s) => new NetworkString() { info = new FixedString32Bytes(s) };
}