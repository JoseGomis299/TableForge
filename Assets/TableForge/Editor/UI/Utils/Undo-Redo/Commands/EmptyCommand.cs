namespace TableForge.Editor.UI
{
    internal class EmptyCommand : BaseUndoableCommand
    {
        public override void Execute() { }
        
        public override void Undo() { }
    }
}