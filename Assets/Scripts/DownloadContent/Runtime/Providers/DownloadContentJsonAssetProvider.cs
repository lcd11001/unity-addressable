using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using UnityEngine;
using UnityEngine.ResourceManagement.ResourceProviders;

namespace DownloadContent.Providers
{
    [DisplayName("DLC JSON Asset Provider")]
    public class DownloadContentJsonAssetProvider : JsonAssetProvider
    {
        private ProvideHandle provideHandle;

        /// <summary>
        /// Unfortunately we have to override this because the method CanProvide is only called once and when the InternalId
        /// changes this Provider is still selected for non-Firebase Json data. We just call into base when that happens
        /// </summary>
        public override string ProviderId
        {
            get
            {
                return typeof(JsonAssetProvider).FullName;
            }
        }

        public override void Provide(ProvideHandle provideHandle)
        {
            this.provideHandle = provideHandle;
            base.Provide(provideHandle);
        }
    }
}
