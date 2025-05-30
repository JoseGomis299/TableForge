namespace TableForge.UI
{
    internal class DeserializeCellCommand : IUndoableCommand
    {
        private readonly object _oldValue;
        private readonly Cell _cell;
        private readonly string _serializedValue;
        
        public DeserializeCellCommand(Cell cell, object oldValue, string serializedValue)
        {
            _cell = cell;
            _serializedValue = serializedValue;
            _oldValue = oldValue;
        }
        
        public void Execute()
        {
            _cell.TryDeserialize(_serializedValue);
        }
        
        public void Undo()
        {
            _cell.SetValue(_oldValue);
        }
    }
}