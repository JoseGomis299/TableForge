using UnityEditor;

namespace TableForge.UI
{
    internal class RenameAssetCommand : IUndoableCommand
    {
        private readonly string _assetGuid;
        private readonly string _oldName;
        private readonly string _newName;
        
        public string Error { get; private set; }

        public RenameAssetCommand(string assetGuid, string oldName, string newName)
        {
            _assetGuid = assetGuid;
            _oldName = oldName;
            _newName = newName;
        }

        public void Execute()
        {
            Error = AssetDatabase.RenameAsset(AssetDatabase.GUIDToAssetPath(_assetGuid), _newName);
        }

        public void Undo()
        {
            Error = AssetDatabase.RenameAsset(AssetDatabase.GUIDToAssetPath(_assetGuid), _oldName);
        }
    }
}