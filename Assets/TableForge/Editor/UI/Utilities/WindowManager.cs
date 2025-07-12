using System.Collections.Generic;
using System.Linq;
using UnityEditor;

namespace TableForge.Editor.UI
{
    public static class WindowManager
    {
        private static List<EditorWindow> OpenModalWindows { get; } = new();
        public static HashSet<EditorWindow> OpenWindows { get; } = new();
        
        public static void ShowModalWindow<T>(T window) where T : EditorWindow
        {
            if (OpenModalWindows.Contains(window)) return;
            
            bool isAnyWindowOpen = OpenModalWindows.Count > 0;
            
            OpenModalWindows.Add(window);
            OpenWindows.Add(window);
            window.ShowUtility();
            window.Focus();
            
            if (!isAnyWindowOpen)
            {
               EditorApplication.update += BlockOtherWindows;
            }
        }
        
        public static void CloseModalWindow<T>(T window) where T : EditorWindow
        {
            if (!OpenModalWindows.Contains(window)) return;
            
            OpenModalWindows.Remove(window);
            OpenWindows.Remove(window);
            
            if (OpenModalWindows.Count == 0)
            {
                EditorApplication.update -= BlockOtherWindows;
            }
        }
        
        private static void BlockOtherWindows()
        {
            if (OpenModalWindows.Count == 0) return;
            
            var focusedWindow = OpenModalWindows[^1];
            if (focusedWindow != null && focusedWindow != EditorWindow.focusedWindow)
            {
                focusedWindow.Focus();
            }

            while (focusedWindow == null && OpenModalWindows.Count > 0)
            {
                OpenModalWindows.RemoveAt(OpenModalWindows.Count - 1);
                focusedWindow = OpenModalWindows.LastOrDefault();
            }
            
            if(OpenModalWindows.Count == 0)
            {
                EditorApplication.update -= BlockOtherWindows;
            }
        }
        
        
    }
}