using System;
using System.Collections;
using System.Collections.Generic;

namespace TableForge.Editor.UI
{
    internal class AddCollectionRowCommand : BaseUndoableCommand, ICellBoundCommand, IAssetBoundCommand
    {
        private readonly Action<TableControl> _addRowAction;
        private readonly TableControl _tableControl;
        private readonly Cell _collectionCell;
        private readonly ICollection _oldCollectionCopy;
        private TableMetadata _oldTableMetadata;
        
        public Cell BoundCell => _collectionCell;
        public List<string> Guids => new() {_collectionCell.row.SerializedObject.RootObjectGuid};
        
        public AddCollectionRowCommand(Action<TableControl> addRowAction, TableControl tableControl, Cell collectionCell, ICollection oldCollectionCopy)
        {
            _addRowAction = addRowAction;
            _tableControl = tableControl;
            _collectionCell = collectionCell;
            _oldCollectionCopy = oldCollectionCopy;
        }
        
        public override void Execute()
        {
            _oldTableMetadata = TableMetadata.Clone(_tableControl.Metadata);
            _addRowAction(_tableControl);
            
            if (_tableControl.Parent is DynamicSubTableCellControl dynamicTableControl)
            {
                _tableControl.SetTable(((SubTableCell)_collectionCell).SubTable);
                dynamicTableControl.OnRowAdded();
            }
        }

        public override void Undo()
        {
            _collectionCell.SetValue(_oldCollectionCopy.CreateShallowCopy());
            var originalMetadata = _tableControl.Metadata;
            TableMetadata.Copy(originalMetadata, _oldTableMetadata);

            if (_tableControl.Parent is DynamicSubTableCellControl dynamicTableControl)
            {
                _tableControl.SetTable(((SubTableCell)_collectionCell).SubTable);
                dynamicTableControl.OnRowDeleted();
            }
        }
    }
}