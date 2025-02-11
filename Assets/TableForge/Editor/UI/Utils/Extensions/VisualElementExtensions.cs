using UnityEngine.UIElements;

namespace TableForge.UI
{
    internal static class VisualElementExtensions
    {
        public static void AddToChildrenClassList(this VisualElement element, string className)
        {
            foreach (var child in element.Children())
            {
                child.AddToClassList(className);
                child.AddToChildrenClassList(className);
            }
        }
        
        public static void RemoveFromChildrenClassList(this VisualElement element, string className)
        {
            foreach (var child in element.Children())
            {
                child.RemoveFromClassList(className);
                child.RemoveFromChildrenClassList(className);
            }
        }
        
    }
}