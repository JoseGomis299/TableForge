using System;

namespace TableForge.UI
{
    internal abstract class ShowTabCommand : IUndoableCommand
    {
        private readonly Action<TabControl> _openTabAction;
        private readonly Action<TabControl> _closeTabAction;
        private readonly TabControl _tab;
        
        protected ShowTabCommand(Action<TabControl> openTabAction, Action<TabControl> closeTabAction, TabControl tab)
        {
            _openTabAction = openTabAction;
            _closeTabAction = closeTabAction;
            _tab = tab;
        }

        public abstract void Execute();
        public abstract void Undo();
        
        protected void OpenTab()
        {
            _openTabAction(_tab);
        }
        
        protected void CloseTab()
        {
            _closeTabAction(_tab);
        }
    }
}