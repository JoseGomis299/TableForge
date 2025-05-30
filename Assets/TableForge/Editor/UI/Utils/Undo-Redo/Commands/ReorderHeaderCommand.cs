using System;

namespace TableForge.UI
{
    internal class ReorderHeaderCommand : IUndoableCommand
    {
        private readonly int _startPosition;
        private readonly int _endPosition;
        private readonly Action<int, int, bool> _reorderAction;
        
        public ReorderHeaderCommand(int startPosition, int endPosition, Action<int, int, bool> reorderAction)
        {
            _startPosition = startPosition;
            _endPosition = endPosition;
            _reorderAction = reorderAction;
        }
        
        public void Execute()
        {
            _reorderAction(_startPosition, _endPosition, true);
        }
        
        public void Undo()
        {
            _reorderAction(_endPosition, _startPosition, true);
        }
    }
}