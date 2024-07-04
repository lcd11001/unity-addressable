using System;
using System.ComponentModel;
using DownloadContent.Services;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.AddressableAssets.ResourceLocators;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceLocations;
using UnityEngine.ResourceManagement.ResourceProviders;

namespace DownloadContent.Providers
{
    [DisplayName("DLC JSON Asset Provider")]
    public class DownloadContentJsonAssetProvider : JsonAssetProvider
    {
        private ProvideHandle provideHandle;

        /// <summary>
        /// Unfortunately we have to override this because the method CanProvide is only called once and when the InternalId
        /// changes this Provider is still selected for non-DLC Json data. We just call into base when that happens
        /// </summary>
        public override string ProviderId => typeof(JsonAssetProvider).FullName;

        public override void Provide(ProvideHandle provideHandle)
        {
            var url = Addressables.ResourceManager.TransformInternalId(provideHandle.Location);
            if (DownloadContentService.IsSupportFormat(url) == false)
            {
                base.Provide(provideHandle);
                return;
            }

            this.provideHandle = provideHandle;
            if (DownloadContentManager.IsInitialized)
            {
                FetchJson();
            }
            else
            {
                DownloadContentManager.OnInitialized += FetchJson;
            }
        }

        private void FetchJson()
        {
            DownloadContentManager.OnInitialized -= FetchJson;
            Debug.Log($"FetchJson: {provideHandle.Location.InternalId}");

            DownloadContentService.GetDlcUrlFromInternalId(provideHandle.Location.InternalId, OnDlcJsonFetched);
        }

        private void OnDlcJsonFetched()
        {
            var url = Addressables.ResourceManager.TransformInternalId(provideHandle.Location);
            Debug.Log($"OnDlcJsonFetched: {url}");

            var jsonLocation = new ResourceLocationBase(url, url, typeof(JsonAssetProvider).FullName, typeof(IResourceLocator));

            if (provideHandle.Location.ResourceType == typeof(ContentCatalogData))
            {
                provideHandle.ResourceManager.ProvideResource<ContentCatalogData>(jsonLocation).Completed += OnJsonLoaded;
            }
            else
            {
                Debug.LogError($"Unsupported resource type: {provideHandle.Location.ResourceType}");
                provideHandle.Complete<IResourceLocator>(null, false, new Exception("Unsupported resource type"));
            }
        }

        private void OnJsonLoaded(AsyncOperationHandle<ContentCatalogData> handle)
        {
            if (handle.Status == AsyncOperationStatus.Succeeded)
            {
                Debug.Log($"OnJsonLoaded: {handle.Result}");
                provideHandle.Complete(handle.Result, true, null);
            }
            else
            {
                Debug.LogError($"Failed to load json: {handle.OperationException}");
                provideHandle.Complete<ContentCatalogData>(null, false, handle.OperationException);
            }
        }
    }
}
