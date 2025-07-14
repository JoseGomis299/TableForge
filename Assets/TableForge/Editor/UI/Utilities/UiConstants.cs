using System;
using UnityEngine;
using UnityEngine.UIElements;

namespace TableForge.Editor.UI
{
    internal static class UiConstants
    {
        public static event Action OnStylesInitialized;

        private static readonly CustomStyleProperty<Color> _borderColor = new("--border-color");
        private static readonly CustomStyleProperty<float> _cellWidth = new("--cell-width");
        private static readonly CustomStyleProperty<float> _cellHeight = new("--cell-height");
        private static readonly CustomStyleProperty<float> _headerPadding = new("--header-padding");
        private static readonly CustomStyleProperty<float> _borderWidth = new("--border-width");
        private static readonly CustomStyleProperty<float> _resizableBorderSpan = new("--resizable-border-span");

        public static Color BorderColor { get; private set; }
        public static float CellWidth { get; private set; }
        public static float CellHeight { get; private set; }
        public static float HeaderHeight { get; private set; }
        public static float SubTableHeaderHeight { get; private set; }
        public static float FoldoutHeight { get; private set; }
        public static float CellContentPadding { get; private set; }
        public static float HeaderPadding { get; private set; }
        public static float BorderWidth { get; private set; }
        public static float ResizableBorderSpan { get; private set; }
        public static float SubTableToolbarWidth { get; private set; }

        public const float TabMinWidth = 50f;
        public const float TabMaxWidth = 400f;
        public const float TabPadding = 20f;
        public const float TabContextButtonWidth = 15f;
        
        public const float BigCellPreferredWidth = 120;
        public const float SmallCellPreferredWidth = 50;
        public const float MinCellWidth = 20;
        public const float MinCellHeight = 25;
        
        public const float SnappingThreshold = 3.5f;
        public const float MoveSelectionStep = 7.5f;
        public const float EnumArrowSize = 14;
        public const float ScrollerWidth = 18;
        public const float ReferenceTypeExtraSpace = 32;
        public const float MaxRecommendedWidth = 800;
        public const float MaxRecommendedHeight = 200;
        public const float SmallHeaderPadding = 5;

        public static void InitializeStyles(VisualElement root)
        {
            root.RegisterCallback<CustomStyleResolvedEvent>(OnCustomStyleResolved);
        }

        private static void OnCustomStyleResolved(CustomStyleResolvedEvent evt)
        {
            BorderColor = evt.customStyle.TryGetValue(_borderColor, out var color) ? color : Color.black;
            
            //This is not working for some reason
            
            // CellWidth = evt.customStyle.TryGetValue(_cellWidth, out var width) ? width : 100;
            // CellHeight = evt.customStyle.TryGetValue(_cellHeight, out var height) ? height : 30;
            // HeaderPadding = evt.customStyle.TryGetValue(_headerPadding, out var padding) ? padding : 10;
            // BorderWidth = evt.customStyle.TryGetValue(_borderWidth, out var borderWidth) ? borderWidth : 1;
            // ResizableBorderSpan = evt.customStyle.TryGetValue(_resizableBorderSpan, out var span) ? span : 5;

            CellWidth = 100;
            CellHeight = 20;
            HeaderHeight = 20;
            SubTableHeaderHeight = 16;
            HeaderPadding = 10;
            BorderWidth = 1;
            ResizableBorderSpan = 5;
            CellContentPadding = 4;
            FoldoutHeight = 20;
            SubTableToolbarWidth = 13;

            OnStylesInitialized?.Invoke();
        }
    }
}
