using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using DownloadContent.Services;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.AddressableAssets.ResourceLocators;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceLocations;
using UnityEngine.ResourceManagement.ResourceProviders;

namespace DownloadContent.Providers
{
    [DisplayName("DLC AssetBundle Provider")]
    public class DownloadContentAssetBundleProvider : AssetBundleProvider
    {
        protected readonly Dictionary<string, AsyncOperationHandle<IAssetBundleResource>> bundleOperationHandlers = new Dictionary<string, AsyncOperationHandle<IAssetBundleResource>>();
        private ProvideHandle provideHandle;
        public override void Provide(ProvideHandle providerInterface)
        {
            string path = providerInterface.ResourceManager.TransformInternalId(providerInterface.Location);
            if (DownloadContentService.IsSupportFormat(path) == false)
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

            IResourceLocation[] dependencies = provideHandle.Location.HasDependencies
                ? provideHandle.Location.Dependencies.ToArray()
                : new IResourceLocation[0];
            var bundleLocation = new ResourceLocationBase(url, url, GetType().FullName, typeof(IResourceLocator), dependencies)
            {
                Data = provideHandle.Location.Data,
                PrimaryKey = provideHandle.Location.PrimaryKey
            };

            AsyncOperationHandle<IAssetBundleResource> asyncOperationHandle;
            if (bundleOperationHandlers.TryGetValue(url, out asyncOperationHandle))
            {
                // Release already running handler
                if (asyncOperationHandle.IsValid())
                {
                    Debug.Log($"Release already running handler: {url}");
                    provideHandle.ResourceManager.Release(asyncOperationHandle);
                }
            }
            Debug.Log($"passing {url} to Unity asset bundle at {bundleLocation.PrimaryKey}");
            asyncOperationHandle = provideHandle.ResourceManager.ProvideResource<IAssetBundleResource>(bundleLocation);
            bundleOperationHandlers.Add(url, asyncOperationHandle);
            asyncOperationHandle.Completed += OnAssetBundleLoaded;
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

        public override void Release(IResourceLocation location, object asset)
        {
            base.Release(location, asset);
            //var url = location.InternalId;
            var url = provideHandle.ResourceManager.TransformInternalId(location);
            Debug.Log($"Release: {url}");
            // We have to make sure that the actual Bundle Load operation for this asset also gets released together with the DLC asset
            if (bundleOperationHandlers.TryGetValue(url, out AsyncOperationHandle<IAssetBundleResource> operation))
            {
                if (operation.IsValid())
                {
                    Addressables.ResourceManager.Release(operation);
                }
                bundleOperationHandlers.Remove(url);
            }
        }
    }
}
