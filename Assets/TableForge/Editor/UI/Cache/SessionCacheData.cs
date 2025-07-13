using UnityEngine;

namespace TableForge.Editor.UI
{
    internal class SessionCacheData : ScriptableObject
    { 
        [HideInInspector] public SerializedHashSet<TableMetadata> openTabs = new();
    }
}