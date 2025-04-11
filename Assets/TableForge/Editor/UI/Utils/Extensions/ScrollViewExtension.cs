using UnityEngine;
using UnityEngine.UIElements;

namespace TableForge.UI
{
    internal static class ScrollViewExtension
    {
        public static void SetScrollbarsVisibility(this ScrollView scrollView, bool show)
        {
            if (show)
            {
                scrollView.horizontalScrollerVisibility = ScrollerVisibility.Auto;
                scrollView.verticalScrollerVisibility = ScrollerVisibility.Auto;
            }
            else
            {
                scrollView.horizontalScrollerVisibility = ScrollerVisibility.Hidden;
                scrollView.verticalScrollerVisibility = ScrollerVisibility.Hidden;
            }
        }
        
        public static void SetVerticalScrollerValue(this ScrollView scrollView, float value)
        {
            scrollView.verticalScroller.highValue = value - scrollView.contentViewport.resolvedStyle.height;
            scrollView.verticalScroller.value = Mathf.Min(value, scrollView.verticalScroller.value);
            scrollView.verticalScroller.Adjust(scrollView.contentViewport.resolvedStyle.height / value);
        }
        
        public static void SetHorizontalScrollerValue(this ScrollView scrollView, float value)
        {
            scrollView.horizontalScroller.highValue = value - scrollView.contentViewport.resolvedStyle.width;
            scrollView.horizontalScroller.value = Mathf.Min(value, scrollView.horizontalScroller.value);
            scrollView.horizontalScroller.Adjust(scrollView.contentViewport.resolvedStyle.width / value);
        }
    }
}