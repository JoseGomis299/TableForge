namespace TableForge.Editor
{
    internal interface ISerializableCell
    {
        string Serialize();
        void Deserialize(string data);
        bool TryDeserialize(string data);
    }
}