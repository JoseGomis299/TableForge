using UnityEngine;

namespace TableForge.Editor
{
    internal class ImportItem
    {
        public string OriginalPath { get; set; }
        public string Path { get; set; }
        public string Guid { get; set; }
        public ScriptableObject ExistingAsset { get; set; }
        public bool WillCreateNew => ExistingAsset == null;
    }
}