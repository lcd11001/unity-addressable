using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using UnityEngine;

namespace DownloadContent.Views
{
    public class DownloadContentMainThread : MonoBehaviour
    {
        private static DownloadContentMainThread _instance;

        public static DownloadContentMainThread Instance
        {
            get
            {
                if (_instance == null)
                {
                    try
                    {
                        GameObject executorObject = new GameObject("DLC Main Thread Executor");
                        _instance = executorObject.AddComponent<DownloadContentMainThread>();
                        DontDestroyOnLoad(executorObject);
                    }
                    catch (Exception e)
                    {
                        Debug.LogError($"Failed to create DownloadContentMainThread: {e}");
                    }
                }
                return _instance;
            }
        }

        // Ensure the constructor is private to enforce singleton usage
        private DownloadContentMainThread() { }

        private static readonly ConcurrentQueue<Action> actions = new ConcurrentQueue<Action>();

        public static void ExecuteInMainThread(Action action)
        {
            actions.Enqueue(action);
        }

        void Update()
        {
            while (actions.TryDequeue(out var action))
            {
                action.Invoke();
            }
        }
    }
}
