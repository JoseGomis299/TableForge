using System.Collections.Generic;

namespace TableForge.Editor.UI
{
    internal class SetFunctionCommand : BaseUndoableCommand, ICellBoundCommand, IAssetBoundCommand
    {
        private string _function;
        private readonly string _oldFunction;
        private readonly int _cellId;
        private readonly TableControl _tableControl;
        private object _oldValue = null; 

        public Cell BoundCell { get; }
        public List<string> Guids => new() {BoundCell.row.SerializedObject.RootObjectGuid};


        public SetFunctionCommand(int cellId, string function, string oldFunction, TableControl tableControl)
        {
            _cellId = cellId;
            _function = function;
            _oldFunction = oldFunction;
            _tableControl = tableControl;

            BoundCell = Editor.CellExtension.GetCellById(tableControl.TableData, cellId);
        }
        
        public override void Execute()
        {
            _oldValue ??= BoundCell.GetValue();
            _tableControl.Metadata.SetFunction(_cellId, _function);
            _tableControl.Visualizer?.ToolbarController?.RefreshFunctionTextField();
        }

        public override void Undo()
        {
            BoundCell.SetValue(_oldValue);
            _tableControl.Metadata.SetFunction(_cellId, _oldFunction);
            _tableControl.Visualizer?.ToolbarController?.RefreshFunctionTextField();
        }
        
        public void Combine(IUndoableCommand command)
        {
            if (command is SetFunctionCommand setFunctionCommand)
            {
                _function = setFunctionCommand._function;
                Execute();
            }
        }

    }
}