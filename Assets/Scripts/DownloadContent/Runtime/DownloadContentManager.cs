using DownloadContent.Controllers;
using DownloadContent.Providers;
using DownloadContent.Views;
using System;
using System.Collections;
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

        public static IEnumerator InitializeCoroutine()
        {
            // Hook default DLC
            Addressables.ResourceManager.ResourceProviders.Add(new DownloadContentAssetBundleProvider());
            Addressables.ResourceManager.ResourceProviders.Add(new DownloadContentJsonAssetProvider());
            Addressables.ResourceManager.ResourceProviders.Add(new DownloadContentHashProvider());

            yield return InitializeCoroutine(DownloadContentController.IdTransformFunc, DownloadContentController.GetWebRequestFunc);
        }

        public static IEnumerator InitializeCoroutine(Func<IResourceLocation, string> internalIdTransformFunc, Action<UnityWebRequest> webRequestOverride)
        {
            if (_isInitialized)
            {
                yield break;
            }

            InternalIdTransformFunc = internalIdTransformFunc;
            WebRequestOverride = webRequestOverride;

            // Wait for the next frame to ensure we are on the main thread
            yield return null;

            InitializeMainThread();

            _isInitialized = true;
            OnInitialized?.Invoke();
        }

        private static void InitializeMainThread()
        {
            // create instance of DownloadContentMainThread
            try
            {
                _ = DownloadContentMainThread.Instance;
            }
            catch (Exception e)
            {
                Debug.LogWarning(e.Message);
            }
            // check if instance is created
            if (DownloadContentMainThread.Exists == false)
            {
                Debug.LogError("Can not create DownloadContentMainThread instance. Please call Initialize method from main thread, such as from Start or Awake.");
                return;
            }
        }
    }
}
