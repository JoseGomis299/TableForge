using System;

namespace TableForge.Editor.UI
{
    internal class RemoveRowCommand : IUndoableCommand
    {
        protected readonly Row row;
        protected readonly TableMetadata oldTableMetadata;
        protected readonly TableControl tableControl;
        protected readonly Action<Row> removeRowAction;
        
        public RemoveRowCommand(Row row, TableMetadata oldTableMetadata, TableControl tableControl, Action<Row> removeRowAction)
        {
            this.row = row;
            this.oldTableMetadata = oldTableMetadata;
            this.tableControl = tableControl;
            this.removeRowAction = removeRowAction;
        }
        
        public virtual void Execute()
        {
            removeRowAction(row);
        }

        public virtual void Undo()
        {
            var originalMetadata = tableControl.Metadata;
            TableMetadata.Copy(originalMetadata, oldTableMetadata);

            Table table = TableMetadataManager.GetTable(originalMetadata);
            tableControl.Visualizer?.ToolbarController.UpdateTableCache(originalMetadata, table);
            tableControl.SetTable(table);
        }
    }
}