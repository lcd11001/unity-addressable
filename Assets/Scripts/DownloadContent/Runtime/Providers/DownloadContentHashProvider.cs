using System;
using System.ComponentModel;
using DownloadContent.Services;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceLocations;
using UnityEngine.ResourceManagement.ResourceProviders;

namespace DownloadContent.Providers
{
    [DisplayName("DLC Hash Provider")]
    public class DownloadContentHashProvider : ResourceProviderBase
    {
        // private ProvideHandle provideHandle;
        private Action initAction = null;
        public override void Provide(ProvideHandle provideInterface)
        {
            // this.provideHandle = provideInterface;

            if (DownloadContentManager.IsInitialized)
            {
                FetchHash(provideInterface);
            }
            else
            {
                initAction = () => FetchHash(provideInterface);
                DownloadContentManager.OnInitialized += initAction;
            }
        }

        private void FetchHash(ProvideHandle provideHandle)
        {
            DownloadContentManager.OnInitialized -= initAction;
            Debug.Log($"FetchHash: {provideHandle.Location.InternalId}");

            DownloadContentService.GetDlcUrlFromInternalId(provideHandle.Location.InternalId, () => OnDlcHashFetched(provideHandle));
        }

        private void OnDlcHashFetched(ProvideHandle provideHandle)
        {
            var url = Addressables.ResourceManager.TransformInternalId(provideHandle.Location);
            Debug.Log($"OnDlcHashFetched: {url}");

            var hashLocation = new ResourceLocationBase(url, url, typeof(TextDataProvider).FullName, typeof(string));
            provideHandle.ResourceManager.ProvideResource<string>(hashLocation).Completed += (handle) => OnHashLoaded(handle, provideHandle);
        }

        private void OnHashLoaded(AsyncOperationHandle<string> handle, ProvideHandle provideHandle)
        {
            if (handle.Status == AsyncOperationStatus.Succeeded)
            {
                Debug.Log($"OnHashLoaded: {handle.Result}");
                provideHandle.Complete(handle.Result, true, null);
            }
            else
            {
                Debug.LogError($"Failed to load hash: {handle.OperationException}");
                provideHandle.Complete<string>(null, false, handle.OperationException);
            }
        }
    }
}
