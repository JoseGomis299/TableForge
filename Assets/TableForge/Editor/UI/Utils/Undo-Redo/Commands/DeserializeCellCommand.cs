namespace TableForge.Editor.UI
{
    internal class DeserializeCellCommand : IUndoableCommand
    {
        private readonly object _oldValue;
        private readonly Cell _cell;
        private readonly string _serializedValue;
        private readonly TableMetadata _metadata;
        private string _oldFunction = null;
        
        public DeserializeCellCommand(Cell cell, object oldValue, string serializedValue, TableMetadata metadata)
        {
            _cell = cell;
            _serializedValue = serializedValue;
            _metadata = metadata;
            _oldValue = oldValue;
        }
        
        public void Execute()
        {
            object oldValue = _cell.GetValue(); 
            if (_cell.TryDeserialize(_serializedValue) && !oldValue.Equals(_serializedValue) && ToolbarData.RemoveFormulaOnCellValueChange)
            {
                _oldFunction = _metadata.GetFunction(_cell.Id);
                _metadata.SetFunction(_cell.Id, null);
            }
        }
        
        public void Undo()
        {
            _cell.SetValue(_oldValue);
            if (_oldFunction != null && ToolbarData.RemoveFormulaOnCellValueChange)
            {
                _metadata.SetFunction(_cell.Id, _oldFunction);
            }
        }
    }
}