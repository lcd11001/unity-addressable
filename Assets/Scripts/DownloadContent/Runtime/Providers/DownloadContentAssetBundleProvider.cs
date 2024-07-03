using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using DownloadContent.Contants;
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
        }
    }
}
