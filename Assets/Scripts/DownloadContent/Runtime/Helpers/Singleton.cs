using System;
using UnityEngine;
using UnityEngine.ResourceManagement.Util;

#if UNITY_EDITOR
using UnityEditor;
using UnityEditorInternal;
#endif

namespace DownloadContent.Helpers
{
    public abstract class Singleton<T> where T : Singleton<T>, new()
    {
        protected static bool reinitializeInstance;
        private static T instance;
        public static T Instance
        {
            get
            {
#if UNITY_EDITOR
                //if (InternalEditorUtility.CurrentThreadIsMainThread() && reinitializeAddressables && EditorSettings.enterPlayModeOptionsEnabled)
                if (reinitializeInstance)
                {
                    Debug.Log($"Reinitializing {typeof(T).Name}");
                    reinitializeInstance = false;
                    instance = null;
                    instantiated = false;
                }
#endif
                if (instance == null)
                {
                    instance = new T();
                    instantiated = true;
                }
                return instance;
            }
        }

        private static bool instantiated;
        protected Singleton()
        {
            if (instantiated)
            {
                throw new Exception($"Please use {typeof(T).Name}.Instance instead of new() operator");
            }
        }
    }
}