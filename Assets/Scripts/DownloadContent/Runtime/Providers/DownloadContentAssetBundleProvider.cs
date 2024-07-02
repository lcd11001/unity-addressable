using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using UnityEngine;
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
            Debug.Log($"Transformed {providerInterface.Location.InternalId} to {path}");

            if (DownloadContentManager.Instance.IsDlcUrl(path) == false)
            {
                Debug.Log($"Not a DLC URL: {path}. Redirect to base Unity provider");
                base.Provide(providerInterface);
                return;
            }

            this.provideHandle = providerInterface;
            if (DownloadContentManager.Instance.IsInitialized)
            {
                LoadResource();
            }
            else
            {
                DownloadContentManager.Instance.OnInitialized += LoadResource;
            }
        }

        private void LoadResource()
        {
            DownloadContentManager.Instance.OnInitialized -= LoadResource;
        }
    }
}
