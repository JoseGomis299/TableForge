using System;
using UnityEngine.UIElements;

namespace TableForge.Editor.UI
{
    internal static class VisualElementExtension
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
        
        public static bool HasChildrenClass(this VisualElement element, string className)
        {
            foreach (var child in element.Children())
            {
                if (child.ClassListContains(className) || child.HasChildrenClass(className))
                    return true;
            }

            return false;
        }
        
        public static void SetImmediateChildrenEnabled(this VisualElement element, bool enabled)
        {
            foreach (var child in element.Children())
            {
                child.SetEnabled(enabled);

                // if (!enabled)
                // {
                //     child.RemoveFromClassList(VisualElement.disabledUssClassName);
                //     child.RegisterSingleUseCallbackOnce<CustomStyleResolvedEvent>(() =>
                //         child.RemoveFromClassList(VisualElement.disabledUssClassName));
                // }
            }
        }
        
        public static void SetChildrenEnabled(this VisualElement element, bool enabled)
        {
            element.SetEnabled(enabled);
            foreach (var child in element.Children())
            {
                child.SetChildrenEnabled(enabled);
            }
        }
     
        
        /// <summary>
        ///  Registers a callback for a single use event.
        ///  Once the event is triggered, the callback is unregistered.
        /// </summary>
        /// <param name="element">The VisualElement where the callback will be registered.</param>
        /// <param name="actionToPerform">The action that will be performed whe the callback is triggered.</param>
        /// <param name="trickleDown">Whether the callback will be called during the trickle down phase.</param>
        /// <typeparam name="T">The type of the callback to be registered.</typeparam>
        public static void RegisterSingleUseCallback<T>(this VisualElement element, Action<T> actionToPerform, TrickleDown trickleDown = TrickleDown.NoTrickleDown) where T : EventBase<T>, new()
        {
            element.RegisterCallback<T>(OnEventPerformed, trickleDown);
            
            void OnEventPerformed(T evt)
            {
                element.UnregisterCallback<T>(OnEventPerformed);
                actionToPerform?.Invoke(evt);
            }
        }
        
        public static void SwapChildren(this VisualElement element, VisualElement child1, VisualElement child2)
        {
            int index1 = element.IndexOf(child1);
            int index2 = element.IndexOf(child2);

            if (index1 == -1 || index2 == -1)
                throw new ArgumentException("One or both elements are not children of the specified parent.");

            element.SwapChildren(index1, index2);
        }
        
        public static void SwapChildren(this VisualElement element, int index1, int index2)
        {
            if (index1 == index2)
                return; 

            if (index1 > index2)
            {
                (index1, index2) = (index2, index1);
            }

            var child1 = element.ElementAt(index1);
            var child2 = element.ElementAt(index2);

            element.RemoveAt(index2);
            element.RemoveAt(index1);

            element.Insert(index1, child2);
            element.Insert(index2, child1);
        }
    }
}