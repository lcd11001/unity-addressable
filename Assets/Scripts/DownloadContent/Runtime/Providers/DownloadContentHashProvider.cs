using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using UnityEngine;
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
        }
    }
}
