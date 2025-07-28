using System.Collections.Generic;

namespace TableForge.Editor.UI
{
    internal class SetCellValueCommand : BaseUndoableCommand, ICellBoundCommand, IAssetBoundCommand
    {
        private readonly Cell _cell;
        private CellControl _cellControl;
        private readonly TableControl _tableControl;
        private readonly object _oldValue;
        private object _newValue;
        private string _oldFunction = null;
        
        public Cell BoundCell => _cell;
        public Cell Cell => _cell;
        public List<string> Guids => new() {BoundCell.row.SerializedObject.RootObjectGuid};
        
        public SetCellValueCommand(Cell cell, CellControl cellControl, object oldValue, object newValue)
        {
            _cell = cell;
            _cellControl = cellControl;
            _oldValue = oldValue;
            _newValue = newValue;
            _tableControl = cellControl?.TableControl;
        }
        
        public SetCellValueCommand(Cell cell, TableControl tableControl, object oldValue, object newValue)
        {
            _cell = cell;
            _tableControl = tableControl;
            _oldValue = oldValue;
            _newValue = newValue;
        }
        
        public override void Execute()
        {
            _cellControl ??= CellControlFactory.GetCellControlFromId(_cell.Id);
            _cell.SetValue(_newValue);
            
            if (TableSettings.GetSettings().removeFormulaOnCellValueChange && _oldFunction == null && _oldValue != null && !_oldValue.Equals(_newValue))
            {
                _oldFunction = _tableControl.Metadata.GetFunction(_cell.Id);
                _tableControl.Metadata.SetFunction(Cell.Id, string.Empty);
                _tableControl.Visualizer?.ToolbarController.RefreshFunctionTextField();
            }
            
            if(_cellControl != null && _cell.Id == _cellControl.Cell.Id) 
                _cellControl.Refresh();
        }
        
        public override void Undo()
        {
            _cellControl ??= CellControlFactory.GetCellControlFromId(_cell.Id);
            _cell.SetValue(_oldValue);
            if(_oldFunction != null && TableSettings.GetSettings().removeFormulaOnCellValueChange)
            {
                _tableControl.Metadata.SetFunction(_cell.Id, _oldFunction);
            }
            
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