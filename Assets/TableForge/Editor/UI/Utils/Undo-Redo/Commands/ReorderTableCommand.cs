namespace TableForge.UI
{
    internal class ReorderTableCommand : IUndoableCommand
    {
        private int[] _oldPositions;
        private int[] _newPositions;
        private TableControl _tableControl;
        
        public ReorderTableCommand(TableControl tableControl, int[] oldPositions, int[] newPositions)
        {
            _tableControl = tableControl;
            _oldPositions = oldPositions;
            _newPositions = newPositions;
        }
        
        public void Execute()
        {
            _tableControl.TableData.SetRowOrder(_newPositions);
            _tableControl.Metadata.SetAnchorPositions(_tableControl.TableData);
            _tableControl.RebuildPage();
        }
        
        public void Undo()
        {
            _tableControl.TableData.SetRowOrder(_oldPositions);
            _tableControl.Metadata.SetAnchorPositions(_tableControl.TableData);
            _tableControl.RebuildPage();
        }
    }
}