using System.Collections;
using System.Collections.Generic;
using UnityEditor.AddressableAssets.Build.DataBuilders;
using UnityEngine;

namespace DownloadContent.Builders
{
    [CreateAssetMenu(fileName = "DLCBuildScript", menuName = "DLC/Content Builders/DLC Build Script")]
    public class DownloadContentBuildScript : BuildScriptPackedMode
    {
        public override string Name => "DLC Build";
    }
}
