using System.Collections.Generic;
using System.Linq;

namespace TableForge.Editor.UI
{
    internal static class UndoRedoManager
    {
        private static readonly Stack<IUndoableCommand> _undoStack = new();
        private static readonly Stack<IUndoableCommand> _redoStack = new();

        private static readonly Stack<CommandCollection> _collections = new();
        private static CommandCollection _currentCollection;

        public static void Do(IUndoableCommand command)
        {
            if (_currentCollection != null)
            {
                _currentCollection.AddAndExecuteCommand(command);
                return;
            }
            
            command.Execute();
            _undoStack.Push(command);
            _redoStack.Clear();
        }

        public static void AddToQueue(IUndoableCommand command)
        {
            if (_currentCollection != null)
            {
                _currentCollection.AddCommand(command);
                return;
            }
            
            _undoStack.Push(command);
            _redoStack.Clear();
        }
        
        public static void Undo(out List<Cell> relatedCells)
        {
            relatedCells = new List<Cell>();
            if (_undoStack.Count == 0) return;
            _currentCollection = null;
            _collections.Clear();
            var cmd = _undoStack.Pop();
            while (cmd is EmptyCommand && _undoStack.Count > 0)
            {
                cmd = _undoStack.Pop();
            }
            
            if (cmd is CommandCollection collection)
            {
                foreach (var cell in collection.BoundCells)
                {
                    if(cell.row.SerializedObject.RootObject != null)
                        relatedCells.Add(cell);
                }
            }
            else if (cmd is ICellBoundCommand cellBoundCommand && cellBoundCommand.BoundCell.row.SerializedObject.RootObject != null)
            {
                relatedCells.Add(cellBoundCommand.BoundCell);
            }
            
            cmd.Undo();
            _redoStack.Push(cmd);
        }

        public static void Redo(out List<Cell> relatedCells)
        {
            relatedCells = new List<Cell>();
            if (_redoStack.Count == 0) return;
            var cmd = _redoStack.Pop();
            
            if (cmd is CommandCollection collection)
            {
                relatedCells.AddRange(collection.BoundCells);
            }
            else if (cmd is ICellBoundCommand cellBoundCommand)
            {
                relatedCells.Add(cellBoundCommand.BoundCell);
            }
            
            cmd.Execute();
            _undoStack.Push(cmd);
        }
        
        public static IUndoableCommand GetLastUndoCommand()
        {
            return _undoStack.Count > 0 ? _undoStack.Peek() : null;
        }
        
        public static void StartCollection()
        {
            _collections.Push(new CommandCollection());
            _currentCollection = _collections.Peek();
            _undoStack.Push(_currentCollection);
        }
        
        public static void EndCollection()
        {
            if(_collections.Count == 0) return;
            if(_currentCollection.CommandTypes.All(t => t == typeof(EmptyCommand)))
            {
                _undoStack.Pop(); // Remove empty collection from undo stack
            }

            _collections.Pop();
            _currentCollection = _collections.Count > 0 ? _collections.Peek() : null;
        }
        
        public static void Clear()
        {
            _undoStack.Clear();
            _redoStack.Clear();
            _collections.Clear();
            _currentCollection = null;
        }

        public static void RemoveRelatedCommandsFromStack(string relatedGuid)
        {
            if (_undoStack.Count == 0) return;

            var commandsToKeep = _undoStack
                .Where(cmd => !cmd.IsRelatedToAsset(relatedGuid))
                .ToList();

            _undoStack.Clear();
            for (var i = commandsToKeep.Count - 1; i >= 0; i--)
            {
                var command = commandsToKeep[i];
                _undoStack.Push(command);
            }

            commandsToKeep = _redoStack
                .Where(cmd => !cmd.IsRelatedToAsset(relatedGuid))
                .ToList();
            
            _redoStack.Clear();
            for (var i = commandsToKeep.Count - 1; i >= 0; i--)
            {
                var command = commandsToKeep[i];
                _redoStack.Push(command);
            }
        }
    }

}