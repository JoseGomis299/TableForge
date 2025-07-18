using UnityEditor;
using UnityEngine;

namespace TableForge.Editor
{
    public static class InspectorChangeNorifier
    {
        public static event System.Action<ScriptableObject> OnScriptableObjectModified;

        [InitializeOnLoadMethod]
        private static void Initialize()
        {
            Undo.postprocessModifications += OnPostprocessModifications;
            AssemblyReloadEvents.beforeAssemblyReload += OnBeforeAssemblyReload;
        }

        private static void OnBeforeAssemblyReload()
        {
            Undo.postprocessModifications -= OnPostprocessModifications;
        }

        private static UndoPropertyModification[] OnPostprocessModifications(UndoPropertyModification[] modifications)
        {
            foreach (var mod in modifications)
            {
                if (mod.currentValue?.target is ScriptableObject scriptableObject)
                {
                    OnScriptableObjectModified?.Invoke(scriptableObject);
                }
            }

            return modifications;
        }
    }
}