using DownloadContent.Providers;
using DownloadContent.Services;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.AddressableAssets.ResourceLocators;
using UnityEngine.Events;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace DownloadContent.Views
{
    public abstract class DownloadContentViewBase : MonoBehaviour
    {
        [SerializeField] DownloadContentServiceBase dlcService;

        public abstract void OnDownloadContentSuccess();
        public abstract void OnDownloadContentFailed(Exception exception);

        protected virtual void Start()
        {
            dlcService.onDownloadContentServicedInitialized += OnDownloadContentServicedInitialized;
            dlcService.Initialize();
        }

        protected virtual void OnDestroy()
        {
            dlcService.onDownloadContentServicedInitialized -= OnDownloadContentServicedInitialized;
            dlcService.Dispose();

            DownloadContentManager.OnInitialized -= OnDownloadContentInitialized;
            Addressables.InitializeAsync().Completed -= OnAddressableInitialized;
        }

        protected virtual void OnDownloadContentServicedInitialized()
        {
            StartCoroutine(InitializeDownloadContent());
        }

        protected virtual IEnumerator InitializeDownloadContent()
        {
            DownloadContentManager.OnInitialized += OnDownloadContentInitialized;

            // Check if an instance of DLCAssetBundleProvider already exists
            if (!DownloadContentService.IsContainsType(Addressables.ResourceManager.ResourceProviders, typeof(DownloadContentAssetBundleProvider)))
            {
                Addressables.ResourceManager.ResourceProviders.Add(new DownloadContentAssetBundleProvider());
            }
            // Check if an instance of DLCJsonAssetProvider already exists
            if (!DownloadContentService.IsContainsType(Addressables.ResourceManager.ResourceProviders, typeof(DownloadContentJsonAssetProvider)))
            {
                Addressables.ResourceManager.ResourceProviders.Add(new DownloadContentJsonAssetProvider());
            }
            // Check if an instance of DLCHashProvider already exists
            if (!DownloadContentService.IsContainsType(Addressables.ResourceManager.ResourceProviders, typeof(DownloadContentHashProvider)))
            {
                Addressables.ResourceManager.ResourceProviders.Add(new DownloadContentHashProvider());
            }

            // override the priority
            DownloadContentService.InsertDownloadContentFetcher(dlcService, 0);
            yield return DownloadContentManager.InitializeCoroutine(dlcService.IdTransformFunc, dlcService.GetWebRequestFunc);
        }

        protected virtual void OnDownloadContentInitialized()
        {
            Debug.Log("DLC Initialized");
            Addressables.InitializeAsync().Completed += OnAddressableInitialized;
        }

        protected virtual void OnAddressableInitialized(AsyncOperationHandle<IResourceLocator> handle)
        {
            if (handle.Status == AsyncOperationStatus.Succeeded)
            {
                Debug.Log("Addressables Initialized");
                OnDownloadContentSuccess();
            }
            else
            {
                Debug.LogError("Failed to initialize Addressables");
                OnDownloadContentFailed(handle.OperationException);
            }
        }
    }
}
