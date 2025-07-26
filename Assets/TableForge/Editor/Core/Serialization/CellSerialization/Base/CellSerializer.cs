using System;

namespace TableForge.Editor.Serialization
{
    internal abstract class CellSerializer : ICellSerializer
    {
        public ISerializer ValueSerializer => serializer;

        protected readonly Cell cell;
        protected ISerializer serializer;
        
        protected CellSerializer(Cell cell)
        {
            this.cell = cell;
            serializer = new JsonSerializer();
        }
        
        public abstract string Serialize(SerializationOptions options);
        public abstract void Deserialize(string data, SerializationOptions options);
        
        public virtual bool TryDeserialize(string data, SerializationOptions options)
        {
            try
            {
                Deserialize(data, options);
                return true;
            }
            catch(Exception)
            {
                return false;
            }
        }
    }
}