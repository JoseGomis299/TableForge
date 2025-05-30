using System;
using System.Collections.Generic;

namespace TableForge.UI
{
    internal class CommandCollection : IUndoableCommand
    {
        private readonly List<IUndoableCommand> _commands;
        private readonly HashSet<Type> _commandTypes;
        
        public IEnumerable<Type> CommandTypes => _commandTypes;
        public int Count => _commands.Count;
        
        public CommandCollection(List<IUndoableCommand> commands = null)
        {
            _commands = commands ?? new List<IUndoableCommand>();
            _commandTypes = new HashSet<Type>();
        }
        
        public void Execute()
        {
            foreach (var command in _commands)
            {
                command.Execute();
            }
        }
        
        public void Undo()
        {
            for (int i = _commands.Count - 1; i >= 0; i--)
            {
                _commands[i].Undo();
            }
        }
        
        public void AddAndExecuteCommand(IUndoableCommand command)
        {
            _commands.Add(command);
            _commandTypes.Add(command.GetType());
            command.Execute();
        }
        
        public void AddCommand(IUndoableCommand command)
        {
            _commands.Add(command);
            _commandTypes.Add(command.GetType());
        }
        
        public void Clear()
        {
            _commands.Clear();
        }
    }
}