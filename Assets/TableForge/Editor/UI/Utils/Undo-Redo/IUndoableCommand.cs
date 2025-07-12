namespace TableForge.Editor.UI
{
    internal interface IUndoableCommand
    {
        void Execute();
        void Undo();
        bool IsRelatedToAsset(string guid);
    }
}