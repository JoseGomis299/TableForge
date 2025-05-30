namespace TableForge.UI
{
    internal class EmptyCommand : IUndoableCommand
    {
        public void Execute() { }
        
        public void Undo() { }
    }
}