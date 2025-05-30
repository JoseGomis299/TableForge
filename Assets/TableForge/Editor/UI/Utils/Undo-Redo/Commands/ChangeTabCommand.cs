using System;

namespace TableForge.UI
{
    internal class ChangeTabCommand : IUndoableCommand
    {
        private readonly TableMetadata _previousTableMetadata;
        private readonly TableMetadata _newTableMetadata;
        private readonly Action<TableMetadata> _changeTabAction;
        
        public ChangeTabCommand(TableMetadata previousTableMetadata, TableMetadata newTableMetadata, Action<TableMetadata> changeTabAction)
        {
            _previousTableMetadata = previousTableMetadata;
            _newTableMetadata = newTableMetadata;
            _changeTabAction = changeTabAction;
        }
        
        public void Execute()
        {
            _changeTabAction(_newTableMetadata);
        }

        public void Undo()
        {
            _changeTabAction(_previousTableMetadata);
        }
    }
}