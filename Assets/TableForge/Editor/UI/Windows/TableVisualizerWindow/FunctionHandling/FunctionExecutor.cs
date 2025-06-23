using System;
using System.Collections.Generic;

namespace TableForge.Editor.UI
{
    internal class FunctionExecutor
    {
        private readonly TableControl _tableControl;
        private readonly FunctionParser _functionParser;
        private readonly Dictionary<int, Action> _cellFunctions;
        private readonly Dictionary<int, Action> _columnFunctions;
        
        public FunctionExecutor(TableControl tableControl)
        {
            _tableControl = tableControl;
            _functionParser = new FunctionParser(this);
            _cellFunctions = new Dictionary<int, Action>();
            _columnFunctions = new Dictionary<int, Action>();
        }

        public void Setup()
        {
            var functions = _tableControl.Metadata.GetFunctions();
            if (functions == null || functions.Count == 0) return;
         
            _cellFunctions.Clear();
            _columnFunctions.Clear();
            
            Table table = _tableControl.TableData;
            TableMetadata metadata = _tableControl.Metadata;
            foreach (var row in table.OrderedRows)
            {
                foreach (var parentCell in row.OrderedCells)
                {
                    foreach (var cell in parentCell.GetDescendants(includeSelf: true))
                    {
                        Column column = cell.Column;
                        string columnFunction = metadata.GetFunction(column.Id);
                        if (!string.IsNullOrWhiteSpace(columnFunction) && !_columnFunctions.ContainsKey(column.Id))
                        {
                            Action action = _functionParser.ParseColumnFunction(columnFunction, column);
                            _columnFunctions[column.Id] = action;
                        }
                        
                        string cellFunction = metadata.GetFunction(cell.Id);
                        if (!string.IsNullOrWhiteSpace(cellFunction) && !_cellFunctions.ContainsKey(cell.Id))
                        {
                            Action action = _functionParser.ParseCellFunction(cellFunction, cell);
                            _cellFunctions[cell.Id] = action;
                        }
                    }
                }
            }
        }
        
        public void SetCellFunction(Cell cell, string function)
        {
            int cellId = cell.Id;
            _tableControl.Metadata.SetFunction(cellId, function);
            if (string.IsNullOrWhiteSpace(function))
            {
                _cellFunctions.Remove(cellId);
                return;
            }

            var action = _functionParser.ParseCellFunction(function, cell);
            if (action != null)
            {
                _cellFunctions[cellId] = action;
            }
        }
        
        public void SetColumnFunction(Column column, string function)
        {
            int columnId = column.Id;
            _tableControl.Metadata.SetFunction(columnId, function);
            if (string.IsNullOrWhiteSpace(function))
            {
                _columnFunctions.Remove(columnId);
                return;
            }

            var action = _functionParser.ParseColumnFunction(function, column);
            if (action != null)
            {
                _columnFunctions[columnId] = action;
            }
        }
        
        public void ExecuteCellFunction(int cellId)
        {
            if (_cellFunctions.TryGetValue(cellId, out var action))
            {
                action.Invoke();
            }
            else
            {
                throw new KeyNotFoundException($"No function found for cell ID {cellId}");
            }
        }
        
        public void ExecuteColumnFunction(int columnId)
        {
            if (_columnFunctions.TryGetValue(columnId, out var action))
            {
                action.Invoke();
            }
            else
            {
                throw new KeyNotFoundException($"No function found for column ID {columnId}");
            }
        }

        public void ExecuteAllFunctions()
        {
            foreach (var action in _columnFunctions.Values)
            {
                action.Invoke();
            }
            
            //Cell functions have priority over column functions, so we execute them last.
            foreach (var action in _cellFunctions.Values)
            {
                action.Invoke();
            }
            
            _tableControl.Update();
        }
    }
}