using System;

namespace TableForge.Editor.UI
{
    internal class CloseTabCommand : ShowTabCommand
    {
        public CloseTabCommand(Action<TabControl> openTabAction, Action<TabControl> closeTabAction, TabControl tab)
            : base(openTabAction, closeTabAction, tab) { }

        public override void Execute()
        {
            CloseTab();
        }

        public override void Undo()
        {
            OpenTab();
        }
    }
}