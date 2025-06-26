using System.Collections.Generic;
using System.Linq;

namespace TableForge.Editor.UI
{
    internal static class UndoRedoManager
    {
        private static readonly Stack<IUndoableCommand> _undoStack = new();
        private static readonly Stack<IUndoableCommand> _redoStack = new();
        private static readonly EmptyCommand _separator = new();

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
        
        public static void AddSeparator()
        {
            if(_undoStack.Count > 0 && _undoStack.Peek() is EmptyCommand) return;
            if (_currentCollection != null)
            {
                _currentCollection.AddCommand(_separator);
                return;
            }
            
            _undoStack.Push(_separator);
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
        
        public static void Undo()
        {
            if (_undoStack.Count == 0) return;
            _currentCollection = null;
            _collections.Clear();
            var cmd = _undoStack.Pop();
            while (cmd is EmptyCommand && _undoStack.Count > 0)
            {
                cmd = _undoStack.Pop();
            }
            
            cmd.Undo();
            _redoStack.Push(cmd);
        }

        public static void Redo()
        {
            if (_redoStack.Count == 0) return;
            var cmd = _redoStack.Pop();
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
    }

}