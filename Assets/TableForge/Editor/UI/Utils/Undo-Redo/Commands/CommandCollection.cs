using System;
using System.Collections.Generic;
using System.Linq;

namespace TableForge.Editor.UI
{
    internal class CommandCollection : BaseUndoableCommand
    {
        private readonly List<IUndoableCommand> _commands;
        private readonly HashSet<Type> _commandTypes;
        
        public List<Cell> BoundCells { get; } = new List<Cell>();
        public IEnumerable<Type> CommandTypes => _commandTypes;
        public int Count => _commands.Count;
        
        public CommandCollection(List<IUndoableCommand> commands = null)
        {
            _commands = commands ?? new List<IUndoableCommand>();
            _commandTypes = new HashSet<Type>();
        }
        
        public override void Execute()
        {
            foreach (var command in _commands)
            {
                command.Execute();
            }
        }
        
        public override void Undo()
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
            
            if (command is ICellBoundCommand cellBoundCommand)
            {
                BoundCells.Add(cellBoundCommand.BoundCell);
            }
        }
        
        public void AddCommand(IUndoableCommand command)
        {
            _commands.Add(command);
            _commandTypes.Add(command.GetType());
            
            if (command is ICellBoundCommand cellBoundCommand)
            {
                BoundCells.Add(cellBoundCommand.BoundCell);
            }
        }
        
        public void Clear()
        {
            _commands.Clear();
        }

        public override bool IsRelatedToAsset(string guid)
        {
            return _commands.Any(command => command.IsRelatedToAsset(guid));
        }
    }
}