using System.Collections.Generic;

namespace TableForge.Editor
{
    internal abstract class ItemSelector
    {
        public abstract List<List<ITfSerializedObject>> GetItemData();
    }
}