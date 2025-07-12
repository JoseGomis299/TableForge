using System;
using System.Collections.Generic;

namespace TableForge.Editor.UI
{
    internal class ReorderHeaderCommand : BaseUndoableCommand, IAssetBoundCommand
    {
        private readonly int _startPosition;
        private readonly int _endPosition;
        private readonly Action<int, int, bool> _reorderAction;

        public List<string> Guids { get; }
    
        
        public ReorderHeaderCommand(int startPosition, int endPosition, Action<int, int, bool> reorderAction, CellAnchor anchor)
        {
            _startPosition = startPosition;
            _endPosition = endPosition;
            _reorderAction = reorderAction;
            
            if(anchor is Row row) Guids = new List<string>{row.SerializedObject.RootObjectGuid};
        }
        
        public override void Execute()
        {
            _reorderAction(_startPosition, _endPosition, true);
        }
        
        public override void Undo()
        {
            _reorderAction(_endPosition, _startPosition, true);
        }
    }
}