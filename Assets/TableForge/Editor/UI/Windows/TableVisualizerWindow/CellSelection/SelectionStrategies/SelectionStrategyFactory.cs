using UnityEngine.UIElements;

namespace TableForge.Editor.UI
{
    /// <summary>
    /// Factory that returns the proper selection strategy based on modifier keys.
    /// </summary>
    internal static class SelectionStrategyFactory
    {
        private static ToggleSelectionStrategy _toggleSelectionStrategy;
        private static MultipleSelectionStrategy _multipleSelectionStrategy;
        private static DefaultSelectionStrategy _defaultSelectionStrategy;
        
        public static ISelectionStrategy GetSelectionStrategy(IMouseEvent evt)
        {
            if (evt.ctrlKey) 
                return _toggleSelectionStrategy ??= new ToggleSelectionStrategy();
            if (evt.shiftKey)
                return _multipleSelectionStrategy ??= new MultipleSelectionStrategy();
            
            return _defaultSelectionStrategy ??= new DefaultSelectionStrategy();
        }

        public static ISelectionStrategy GetSelectionStrategy<T>() where T :ISelectionStrategy
        {
            if(typeof(T) == typeof(ToggleSelectionStrategy))
                return _toggleSelectionStrategy ??= new ToggleSelectionStrategy();
            if(typeof(T) == typeof(MultipleSelectionStrategy))
                return _multipleSelectionStrategy ??= new MultipleSelectionStrategy();
            
            return _defaultSelectionStrategy ??= new DefaultSelectionStrategy();
        }
        
        public static ISelectionStrategy GetSelectionStrategy(bool isCtrl, bool isShift)
        {
            if (isCtrl) 
                return _toggleSelectionStrategy ??= new ToggleSelectionStrategy();
            if (isShift)
                return _multipleSelectionStrategy ??= new MultipleSelectionStrategy();
            
            return _defaultSelectionStrategy ??= new DefaultSelectionStrategy();
        }
    }
}