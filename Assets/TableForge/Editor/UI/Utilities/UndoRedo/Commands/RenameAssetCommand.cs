using System.Collections.Generic;
using UnityEditor;

namespace TableForge.Editor.UI
{
    internal class RenameAssetCommand : BaseUndoableCommand, IAssetBoundCommand
    {
        private readonly string _assetGuid;
        private readonly string _oldName;
        private readonly string _newName;
        
        public string Error { get; private set; }
        public List<string> Guids => new() {_assetGuid};

        public RenameAssetCommand(string assetGuid, string oldName, string newName)
        {
            _assetGuid = assetGuid;
            _oldName = oldName;
            _newName = newName;
        }

        public override void Execute()
        {
            Error = AssetDatabase.RenameAsset(AssetDatabase.GUIDToAssetPath(_assetGuid), _newName);
        }

        public override void Undo()
        {
            Error = AssetDatabase.RenameAsset(AssetDatabase.GUIDToAssetPath(_assetGuid), _oldName);
        }
    }
}