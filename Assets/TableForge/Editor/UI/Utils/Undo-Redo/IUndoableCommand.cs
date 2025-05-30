namespace TableForge.UI
{
    internal interface IUndoableCommand
    {
        void Execute();
        void Undo();
    }
}