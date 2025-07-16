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
        
        public abstract string Serialize();
        public abstract void Deserialize(string data);
        
        public virtual bool TryDeserialize(string data)
        {
            try
            {
                Deserialize(data);
                return true;
            }
            catch(Exception e)
            {
                return false;
            }
        }
    }
}