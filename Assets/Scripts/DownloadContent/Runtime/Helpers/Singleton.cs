using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DownloadContent.Helpers
{
    public abstract class Singleton<T> where T : Singleton<T>, new()
    {
        private static readonly Lazy<T> instance = new Lazy<T>(() => new T());
        public static T Instance { get => instance.Value; }

        private static bool instantiated;
        protected Singleton()
        {
            if (instantiated)
            {
                throw new Exception($"Please use {typeof(T).Name}.Instance instead of new() operator");
            }
            instantiated = true;
        }
    }
}
