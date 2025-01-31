using System.Collections.Generic;

namespace TableForge
{
    internal abstract class ItemSelector
    {
        public abstract List<List<ITFSerializedObject>> GetItemData();
    }
}