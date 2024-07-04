using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.ResourceManagement.Util;

namespace DownloadContent.Views
{
    public class DownloadContentMainThread : ComponentSingleton<DownloadContentMainThread>
    {
        // Ensure the constructor is private to enforce singleton usage
        private DownloadContentMainThread() { }

        protected override string GetGameObjectName() => "DLC Main Thread";

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
