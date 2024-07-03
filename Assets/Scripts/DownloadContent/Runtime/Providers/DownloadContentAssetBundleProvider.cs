using System.ComponentModel;
using System.Linq;
using DownloadContent.Contants;
using DownloadContent.Services;
using UnityEngine;
using UnityEngine.AddressableAssets.ResourceLocators;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceLocations;
using UnityEngine.ResourceManagement.ResourceProviders;

namespace DownloadContent.Providers
{
    [DisplayName("DLC AssetBundle Provider")]
    public class DownloadContentAssetBundleProvider : AssetBundleProvider
    {
        private ProvideHandle provideHandle;
        public override void Provide(ProvideHandle providerInterface)
        {
            string path = providerInterface.ResourceManager.TransformInternalId(providerInterface.Location);
            if (DownloadContentConstants.IsDlcUrl(path) == false)
            {
                Debug.Log($"Not a DLC URL: {path}. Redirect to base Unity provider");
                base.Provide(providerInterface);
                return;
            }

            this.provideHandle = providerInterface;
            if (DownloadContentManager.IsInitialized)
            {
                LoadResource();
            }
            else
            {
                DownloadContentManager.OnInitialized += LoadResource;
            }
        }

        private void LoadResource()
        {
            DownloadContentManager.OnInitialized -= LoadResource;
            Debug.Log($"LoadResource: {provideHandle.Location.InternalId}");

            DownloadContentService.GetDlcUrlFromLocation(provideHandle.Location, OnDlcAssetBundleFetched);
        }

        private void OnDlcAssetBundleFetched()
        {
            var url = provideHandle.ResourceManager.TransformInternalId(provideHandle.Location);
            Debug.Log($"OnDlcAssetBundleFetched: {url}");

            var bundleLocation = new ResourceLocationBase(url, url, GetType().FullName, typeof(IResourceLocator), provideHandle.Location.Dependencies.ToArray())
            {
                Data = provideHandle.Location.Data,
                PrimaryKey = provideHandle.Location.PrimaryKey
            };

            provideHandle.ResourceManager.ProvideResource<IAssetBundleResource>(bundleLocation).Completed += OnAssetBundleLoaded;
        }

        private void OnAssetBundleLoaded(AsyncOperationHandle<IAssetBundleResource> handle)
        {
            if (handle.Status == AsyncOperationStatus.Succeeded)
            {
                Debug.Log($"OnAssetBundleLoaded: {handle.Result}");
                provideHandle.Complete(handle.Result, true, null);
            }
            else
            {
                Debug.LogError($"Failed to load asset bundle: {handle.OperationException}");
                provideHandle.Complete<IAssetBundleResource>(null, false, handle.OperationException);
            }
        }
    }
}
