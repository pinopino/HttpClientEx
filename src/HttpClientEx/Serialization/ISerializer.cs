namespace HttpClientEx.Serialization
{
    public interface ISerializer
    {
        byte[] SerializeToBytes(object item);

        string Serialize(object item);

        object Deserialize(byte[] serializedBytes);

        T Deserialize<T>(byte[] serializedBytes);

        object Deserialize(string serializedStr);

        T Deserialize<T>(string serializedStr);
    }
}
