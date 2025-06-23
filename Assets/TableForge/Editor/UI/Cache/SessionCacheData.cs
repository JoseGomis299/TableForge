using UnityEngine;
using UnityEngine.Serialization;

namespace TableForge.Editor.UI
{
    internal class SessionCacheData : ScriptableObject
    {
       [FormerlySerializedAs("OpenTabs")] public SerializedHashSet<TableMetadata> openTabs = new SerializedHashSet<TableMetadata>();
    }
}