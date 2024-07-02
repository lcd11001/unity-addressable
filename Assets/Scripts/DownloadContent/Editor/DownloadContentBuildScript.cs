using DownloadContent.Providers;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor.AddressableAssets.Build;
using UnityEditor.AddressableAssets.Build.DataBuilders;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.AddressableAssets.Initialization;
using UnityEngine.AddressableAssets.ResourceLocators;

namespace DownloadContent.Builders
{
    /// <summary>
    /// Modifies the settings so we use the DLC Proxy Loaders to get the Remote Data files
    /// </summary>
    [CreateAssetMenu(fileName = "DLCBuildScript", menuName = "DLC/Content Builders/DLC Build Script")]
    public class DownloadContentBuildScript : BuildScriptPackedMode
    {
        public override string Name => "DLC Build";

        protected override TResult DoBuild<TResult>(AddressablesDataBuilderInput builderInput, AddressableAssetsBuildContext aaContext)
        {
            var result = base.DoBuild<TResult>(builderInput, aaContext);
            var settingsPath = Path.Join(Addressables.BuildPath, builderInput.RuntimeSettingsFilename);
            var data = JsonUtility.FromJson<ResourceManagerRuntimeData>(File.ReadAllText(settingsPath));

            var remoteHash = data.CatalogLocations.Find(
                locationData => locationData.Keys[0] == "AddressablesMainContentCatalogRemoteHash"
            );

            if (remoteHash != null)
            {
                var newRemoteHash = new ResourceLocationData(
                    remoteHash.Keys,
                    remoteHash.InternalId,
                    typeof(DownloadContentHashProvider),
                    remoteHash.ResourceType,
                    remoteHash.Dependencies
                );

                data.CatalogLocations.Remove(remoteHash);
                data.CatalogLocations.Add(newRemoteHash);

                File.WriteAllText(settingsPath, JsonUtility.ToJson(data));
            }

            return result;
        }
    }
}
