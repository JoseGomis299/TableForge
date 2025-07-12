using System;
using System.Collections.Generic;
using UnityEditor;

namespace TableForge.Editor.UI
{
    internal abstract class ShowTabCommand : BaseUndoableCommand, IAssetBoundCommand
    {
        private readonly Action<TabControl> _openTabAction;
        private readonly Action<TabControl> _closeTabAction;
        private readonly TabControl _tab;
        
        public List<string> Guids => new() {AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(_tab.TableMetadata))};
        
        protected ShowTabCommand(Action<TabControl> openTabAction, Action<TabControl> closeTabAction, TabControl tab)
        {
            _openTabAction = openTabAction;
            _closeTabAction = closeTabAction;
            _tab = tab;
        }

        public abstract override void Execute();
        public abstract override void Undo();
        
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