using System.Collections.Generic;
using UnityEngine;

namespace TableForge.UI
{
    internal class SessionCacheData : ScriptableObject
    {
       public SerializedHashSet<TableMetadata> OpenTabs = new SerializedHashSet<TableMetadata>();
    }
}