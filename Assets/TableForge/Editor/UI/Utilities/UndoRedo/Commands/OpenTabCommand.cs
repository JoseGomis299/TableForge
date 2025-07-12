using System;

namespace TableForge.Editor.UI
{
    internal class OpenTabCommand : ShowTabCommand
    {
        public OpenTabCommand(Action<TabControl> openTabAction, Action<TabControl> closeTabAction, TabControl tab)
            : base(openTabAction, closeTabAction, tab) { }

        public override void Execute()
        {
            OpenTab();
        }

        public override void Undo()
        {
            CloseTab();
        }
    }
}