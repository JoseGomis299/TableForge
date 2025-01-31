using System.Collections.Generic;

namespace TableForge
{
    //WIP
    internal abstract class ItemSelector
    {
        public abstract void Open();
        public virtual void Close() { /* Default implementation */ }
        
        public abstract List<List<ITFSerializedObject>> GetItemData();
    }
}