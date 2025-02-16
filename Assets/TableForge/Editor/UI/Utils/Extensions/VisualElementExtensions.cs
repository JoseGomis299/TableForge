using System;
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
     
        
        /// <summary>
        ///  Registers a callback for a single use event.
        ///  Once the event is triggered, the callback is unregistered.
        /// </summary>
        /// <param name="element">The VisualElement where the callback will be registered.</param>
        /// <param name="actionToPerform">The action that will be performed whe the callback is triggered.</param>
        /// <param name="trickleDown">Whether the callback will be called during the trickle down phase.</param>
        /// <typeparam name="T">The type of the callback to be registered.</typeparam>
        public static void RegisterSingleUseCallback<T>(this VisualElement element, Action actionToPerform, TrickleDown trickleDown = TrickleDown.NoTrickleDown) where T : EventBase<T>, new()
        {
            element.RegisterCallback<T>(OnEventPerformed, trickleDown);
            
            void OnEventPerformed(T evt)
            {
                element.UnregisterCallback<T>(OnEventPerformed);
                actionToPerform?.Invoke();
            }
        }
    }
}