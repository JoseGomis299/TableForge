using UnityEngine;

namespace TableForge.UI
{
    using System;
    using System.Collections.Concurrent;
    using UnityEditor;

    public class ThreadDispatcher
    {
        private  readonly ConcurrentQueue<Action> MainThreadActions = new ConcurrentQueue<Action>();

        public ThreadDispatcher()
        {
            EditorApplication.update += ProcessQueue;
        }

        public void RunOnMainThread(Action action)
        {
            if (action == null) return;
            MainThreadActions.Enqueue(action);
        }

        private void ProcessQueue()
        {
            while (MainThreadActions.TryDequeue(out var action))
            {
                try
                {
                    Debug.Log("Running action on main thread");
                    action.Invoke();
                }
                catch (Exception ex)
                {
                    UnityEngine.Debug.LogException(ex);
                }
            }
        }
        
        public void Dispose()
        {
            EditorApplication.update -= ProcessQueue;
            MainThreadActions.Clear();
        }
    }
}