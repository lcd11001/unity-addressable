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
        private ProvideHandle provideHandle;
        public override void Provide(ProvideHandle provideHandle)
        {
            this.provideHandle = provideHandle;

            if (DownloadContentManager.IsInitialized)
            {
                FetchHash();
            }
            else
            {
                DownloadContentManager.OnInitialized += FetchHash;
            }
        }

        private void FetchHash()
        {
            DownloadContentManager.OnInitialized -= FetchHash;
            Debug.Log($"FetchHash: {provideHandle.Location.InternalId}");

            DownloadContentService.GetDlcUrlFromInternalId(provideHandle.Location.InternalId, OnDlcHashFetched);
        }

        private void OnDlcHashFetched()
        {
            var url = Addressables.ResourceManager.TransformInternalId(provideHandle.Location);
            Debug.Log($"OnDlcHashFetched: {url}");

            var hashLocation = new ResourceLocationBase(url, url, typeof(TextDataProvider).FullName, typeof(string));
            provideHandle.ResourceManager.ProvideResource<string>(hashLocation).Completed += OnHashLoaded;
        }

        private void OnHashLoaded(AsyncOperationHandle<string> handle)
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
