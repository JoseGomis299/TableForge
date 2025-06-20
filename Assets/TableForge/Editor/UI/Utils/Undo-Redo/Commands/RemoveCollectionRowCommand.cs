using System;
using System.Collections;

namespace TableForge.UI
{
    internal class RemoveCollectionRowCommand : RemoveRowCommand
    {
        private readonly Cell _collectionCell;
        private readonly ICollection _oldCollectionCopy;

        public RemoveCollectionRowCommand(Row row, TableMetadata oldTableMetadata, TableControl tableControl, Action<Row> removeRowAction, Cell collectionCell, ICollection oldCollectionCopy) : base(row, oldTableMetadata, tableControl, removeRowAction)
        {
            _oldCollectionCopy = oldCollectionCopy;
            _collectionCell = collectionCell;
        }

        public override void Execute()
        {
            base.Execute();
            
            if (_tableControl.Parent is DynamicTableControl dynamicTableControl)
            {
                _tableControl.SetTable(((SubTableCell)_collectionCell).SubTable);
                dynamicTableControl.OnRowDeleted();
            }
        }

        public override void Undo()
        {
            _collectionCell.SetValue(_oldCollectionCopy.CreateShallowCopy());
            var originalMetadata = _tableControl.Metadata;
            TableMetadata.Copy(originalMetadata, _oldTableMetadata);

            if (_tableControl.Parent is DynamicTableControl dynamicTableControl)
            {
                _tableControl.SetTable(((SubTableCell)_collectionCell).SubTable);
                dynamicTableControl.OnRowAdded();
            }
        }
    }
}