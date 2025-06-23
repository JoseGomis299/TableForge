using UnityEngine;

namespace TableForge.Editor.UI
{
    internal class SessionCacheData : ScriptableObject
    {
       public SerializedHashSet<TableMetadata> OpenTabs = new SerializedHashSet<TableMetadata>();
    }
}