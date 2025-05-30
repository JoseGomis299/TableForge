using System;

namespace TableForge.UI
{
    internal class RemoveRowCommand : IUndoableCommand
    {
        protected readonly Row _row;
        protected readonly TableMetadata _oldTableMetadata;
        protected readonly TableControl _tableControl;
        protected readonly Action<Row> _removeRowAction;
        
        public RemoveRowCommand(Row row, TableMetadata oldTableMetadata, TableControl tableControl, Action<Row> removeRowAction)
        {
            _row = row;
            _oldTableMetadata = oldTableMetadata;
            _tableControl = tableControl;
            _removeRowAction = removeRowAction;
        }
        
        public virtual void Execute()
        {
            _removeRowAction(_row);
        }

        public virtual void Undo()
        {
            var originalMetadata = _tableControl.Metadata;
            TableMetadata.Copy(originalMetadata, _oldTableMetadata);

            Table table = TableMetadataManager.GetTable(originalMetadata);
            _tableControl.Visualizer.ToolbarController.UpdateTableCache(originalMetadata, table);
            _tableControl.SetTable(table);
        }
    }
}