using System;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;

namespace TableForge.Editor.UI
{
    internal class FunctionExecutor
    {
        private readonly TableControl _tableControl;
        private readonly FunctionParser _functionParser;
        private readonly Dictionary<string, Func<object>> _cachedFunctions;
        
        public FunctionExecutor(TableControl tableControl)
        {
            _tableControl = tableControl;
            _functionParser = new FunctionParser(this);
            _cachedFunctions = new Dictionary<string, Func<object>>();
        }

        public void Setup()
        {
            var functions = _tableControl.Metadata.GetFunctions();
            if (functions == null || functions.Count == 0) return;
         
            _cachedFunctions.Clear();
            
            Table table = _tableControl.TableData;
            foreach (var function in functions.Values)
            {
                if (!_cachedFunctions.ContainsKey(function))
                {
                    var cellFunction = _functionParser.ParseCellFunction(function, table);
                    if (cellFunction != null)
                        _cachedFunctions[function] = cellFunction;
                }
            }
        }
        
        public void SetCellFunction(Cell cell, string function)
        {
            int cellId = cell.Id;
            string oldFunction = _tableControl.Metadata.GetFunction(cellId);
            if(function == oldFunction)
            {
                return; // No change, nothing to do
            }
            
            EditFunctionCommand command = new EditFunctionCommand(cellId, function, oldFunction, _tableControl.Metadata, _tableControl.Visualizer.ToolbarController);
            UndoRedoManager.Do(command);
            
            if (!_cachedFunctions.ContainsKey(function))
            {
                var cellFunction = _functionParser.ParseCellFunction(function, _tableControl.TableData);
                if (cellFunction != null)
                    _cachedFunctions[function] = cellFunction;
            }
        }
        
        public void ExecuteCellFunction(int cellId)
        {
            var function = GetFunction(cellId);
            Cell cell = Editor.CellExtension.GetCellById(_tableControl.TableData, cellId);

            if (function != null && cell != null)
            {
                object result = function.Invoke();

                try
                {
                    if(cell.Type.IsAssignableFrom(result.GetType()))
                    {
                        cell.SetValue(result);
                        return;
                    }
                    
                    object properTypeResult = Convert.ChangeType(result, cell.Type, CultureInfo.InvariantCulture);
                    cell.SetValue(properTypeResult);
                }
                catch (Exception e)
                {
                    Debug.LogError($"Function evaluation error for input: {_tableControl.Metadata.GetFunction(cellId)}\n" +
                                   $"Expected type: {cell.Type}, but got: {result?.GetType()}\n" +
                                   $"Error: {e.Message}");
                }
            }
        }

        public void ExecuteAllFunctions()
        {
            var functions = _tableControl.Metadata.GetFunctions();

            foreach (var id in functions.Keys)
            {
                ExecuteCellFunction(id);
            }
            
            _tableControl.Update();
        }

        private Func<object> GetFunction(int id)
        {
            string function = _tableControl.Metadata.GetFunction(id);
            if (string.IsNullOrEmpty(function))
            {
                return null;
            }
            
            if (_cachedFunctions.TryGetValue(function, out var cachedFunction))
            {
                return cachedFunction;
            }
            
            var parsedFunction = _functionParser.ParseCellFunction(function, _tableControl.TableData);
            if (parsedFunction != null)
            {
                _cachedFunctions[function] = parsedFunction;
                return parsedFunction;
            }

            return null;
        }
    }
}