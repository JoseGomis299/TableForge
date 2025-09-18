using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace TableForge.Editor
{
    public static class InspectorChangeNotifier
    {
        public static event System.Action<ScriptableObject> OnScriptableObjectModified;
        private static readonly HashSet<ScriptableObject> _modifiedObjects = new();

        [InitializeOnLoadMethod]
        private static void Initialize()
        {
            Undo.postprocessModifications += OnPostprocessModifications;
            Undo.undoRedoPerformed += OnUndoRedoPerformedCallback;
            AssemblyReloadEvents.beforeAssemblyReload += OnBeforeAssemblyReload;
        }

        private static void OnBeforeAssemblyReload()
        {
            Undo.postprocessModifications -= OnPostprocessModifications;
            Undo.undoRedoPerformed -= OnUndoRedoPerformedCallback;
        }

        private static UndoPropertyModification[] OnPostprocessModifications(UndoPropertyModification[] modifications)
        {
            foreach (var mod in modifications)
            {
                if (mod.currentValue?.target is ScriptableObject scriptableObject)
                {
                    _modifiedObjects.Add(scriptableObject);
                    OnScriptableObjectModified?.Invoke(scriptableObject);
                }
            }

            return modifications;
        }

        private static void OnUndoRedoPerformedCallback()
        {
            foreach (var scriptableObject in _modifiedObjects)
            {
                if (scriptableObject != null) 
                {
                    OnScriptableObjectModified?.Invoke(scriptableObject);
                }
                else
                {
                    // Clean up null references
                    _modifiedObjects.Remove(scriptableObject);
                }
            }
        }
    }
    
    
}