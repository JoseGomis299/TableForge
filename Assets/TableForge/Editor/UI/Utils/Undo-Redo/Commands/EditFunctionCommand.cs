namespace TableForge.Editor.UI
{
    internal class EditFunctionCommand : IUndoableCommand
    {
        private readonly string _function;
        private readonly string _oldFunction;
        private readonly int _cellId;
        private readonly TableMetadata _metadata;
        private readonly ToolbarController _toolbarController;
        
        public EditFunctionCommand(int cellId, string function, string oldFunction, TableMetadata metadata, ToolbarController toolbarController)
        {
            _cellId = cellId;
            _function = function;
            _oldFunction = oldFunction;
            _metadata = metadata;
            _toolbarController = toolbarController;
        }
        
        public void Execute()
        {
            _metadata.SetFunction(_cellId, _function);
            _toolbarController.RefreshFunctionTextField();
        }

        public void Undo()
        {
            _metadata.SetFunction(_cellId, _oldFunction);
            _toolbarController.RefreshFunctionTextField();
        }
    }
}