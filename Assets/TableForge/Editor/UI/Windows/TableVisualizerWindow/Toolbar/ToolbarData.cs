namespace TableForge.Editor.UI
{
    internal class ToolbarData
    {
        public static double RefreshRate { get; set; } = 0.5;
        public static bool EnablePolling { get; set; } = false;
        public static int SubTableMinScrollDiff { get; set; } = 10;
        public static bool EnableUnityTypesTables { get; set; } = false;
        public static bool RemoveFormulaOnCellValueChange { get; set; } = false;
    }
}