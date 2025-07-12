using System.Collections.Generic;

namespace TableForge.Editor.UI
{
    internal class ReorderTableCommand : BaseUndoableCommand, IAssetBoundCommand
    {
        private readonly int[] _oldPositions;
        private readonly int[] _newPositions;
        private readonly TableControl _tableControl;

        public List<string> Guids => new(); // No specific asset bound, so returning an empty list as a wildcard.
        
        public ReorderTableCommand(TableControl tableControl, int[] oldPositions, int[] newPositions)
        {
            _tableControl = tableControl;
            _oldPositions = oldPositions;
            _newPositions = newPositions;
        }
        
        public override void Execute()
        {
            _tableControl.TableData.SetRowOrder(_newPositions);
            _tableControl.Metadata.SetAnchorPositions(_tableControl.TableData);
            _tableControl.RebuildPage();
        }
        
        public override void Undo()
        {
            _tableControl.TableData.SetRowOrder(_oldPositions);
            _tableControl.Metadata.SetAnchorPositions(_tableControl.TableData);
            _tableControl.RebuildPage();
        }
    }
}