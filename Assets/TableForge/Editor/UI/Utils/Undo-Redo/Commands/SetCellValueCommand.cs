namespace TableForge.Editor.UI
{
    internal class SetCellValueCommand : IUndoableCommand
    {
        private readonly Cell _cell;
        private readonly CellControl _cellControl;
        private readonly object _oldValue;
        private object _newValue;
        
        public Cell Cell => _cell;
        
        public SetCellValueCommand(Cell cell, CellControl cellControl, object oldValue, object newValue)
        {
            _cell = cell;
            _cellControl = cellControl;
            _oldValue = oldValue;
            _newValue = newValue;
        }
        
        public void Execute()
        {
            _cell.SetValue(_newValue);
            if(_cellControl != null && _cell.Id == _cellControl.Cell.Id) 
                _cellControl.Refresh();
        }
        
        public void Undo()
        {
            _cell.SetValue(_oldValue);
            if(_cellControl != null && _cell.Id == _cellControl.Cell.Id) 
                _cellControl.Refresh();
        }

        public void Combine(IUndoableCommand command)
        {
            if (command is SetCellValueCommand setCellValueCommand)
            {
                _newValue = setCellValueCommand._newValue;
                Execute();
            }
        }
    }
}