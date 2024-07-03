using DownloadContent.Contants;
using DownloadContent.Controllers;
using DownloadContent.Services;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Networking;
using UnityEngine.ResourceManagement.ResourceLocations;

namespace DownloadContent
{
    public class DownloadContentManager
    {
        private static bool _isInitialized = false;
        public static bool IsInitialized => _isInitialized;

        public static event Action OnInitialized;

        protected static Func<IResourceLocation, string> InternalIdTransformFunc
        {
            get => Addressables.ResourceManager.InternalIdTransformFunc;
            private set
            {
                Addressables.ResourceManager.InternalIdTransformFunc = value;
            }
        }

        protected static Action<UnityWebRequest> WebRequestOverride
        {
            get => Addressables.ResourceManager.WebRequestOverride;
            private set
            {
                Addressables.ResourceManager.WebRequestOverride = value;
            }
        }

        public static void Initialize()
        {
            Initialize(DownloadContentController.IdTransformFunc, DownloadContentController.GetWebRequestFunc);
        }

        public static void Initialize(Func<IResourceLocation, string> internalIdTransformFunc, Action<UnityWebRequest> webRequestOverride)
        {
            if (_isInitialized)
            {
                return;
            }

            InternalIdTransformFunc = internalIdTransformFunc;
            WebRequestOverride = webRequestOverride;

            _isInitialized = true;
            OnInitialized?.Invoke();
        }
    }
}
