using System;
using System.Collections.Generic;
using UnityEditor;

namespace TableForge.Editor.UI
{
    internal class ChangeTabCommand : BaseUndoableCommand, IAssetBoundCommand
    {
        private readonly TableMetadata _previousTableMetadata;
        private readonly TableMetadata _newTableMetadata;
        private readonly Action<TableMetadata> _changeTabAction;
        
        public List<string> Guids => new(){AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(_previousTableMetadata)),
                                           AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(_newTableMetadata))};
        
        public ChangeTabCommand(TableMetadata previousTableMetadata, TableMetadata newTableMetadata, Action<TableMetadata> changeTabAction)
        {
            _previousTableMetadata = previousTableMetadata;
            _newTableMetadata = newTableMetadata;
            _changeTabAction = changeTabAction;
        }
        
        public override void Execute()
        {
            _changeTabAction(_newTableMetadata);
        }

        public override void Undo()
        {
            _changeTabAction(_previousTableMetadata);
        }
    }
}