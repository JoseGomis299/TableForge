namespace TableForge.Editor.UI
{
    internal abstract class BaseUndoableCommand : IUndoableCommand
    {
        public abstract void Execute();
        public abstract void Undo();
        
        public virtual bool IsRelatedToAsset(string guid)
        {
            if (this is IAssetBoundCommand assetBoundCommand)
            {
                if (assetBoundCommand.Guids == null) return false;
                return assetBoundCommand.Guids.Count == 0 /*wildcard*/ || assetBoundCommand.Guids.Contains(guid);
            }

            return false;
        }
    }
}