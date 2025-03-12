using System;

namespace TableForge.UI
{
    internal class ToolbarData
    {
        public static event Action OnPageSizeChanged;
        
        private static int _pageSize = 50;
        public static int PageSize 
        {
            get => _pageSize;
            set
            {
                _pageSize = value;
                OnPageSizeChanged?.Invoke();
            }
        }
        public static int PageStep { get; set; } = 1;
        public static double RefreshRate { get; set; } = 0.5;

        public static bool EnablePolling { get; set; } = false;
    }
}