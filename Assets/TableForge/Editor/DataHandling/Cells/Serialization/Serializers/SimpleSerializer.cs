namespace TableForge.Editor
{
    internal class SimpleSerializer : ISerializer
    {
        public string Serialize<T>(T data)
        {
            return data.ToString();
        }

        public T Deserialize<T>(string data)
        {
            return (T)System.Convert.ChangeType(data, typeof(T));
        }
    }
}