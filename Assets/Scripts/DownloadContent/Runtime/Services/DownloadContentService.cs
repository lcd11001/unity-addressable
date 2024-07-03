using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using DownloadContent.Contants;
using DownloadContent.Controllers;
using UnityEngine.AddressableAssets;
using UnityEngine.AddressableAssets.ResourceLocators;
using UnityEngine.ResourceManagement;
using UnityEngine.ResourceManagement.ResourceLocations;

using Debug = UnityEngine.Debug;

namespace DownloadContent.Services
{
    public class DownloadContentService
    {
        private static int _fetchCount = 0;
        public static void GetDlcUrl(List<object> keys, Action onComplete)
        {
            foreach (var key in keys)
            {
                foreach (IResourceLocator locator in Addressables.ResourceLocators)
                {
                    if (locator.Locate(key, typeof(object), out IList<IResourceLocation> locations))
                    {
                        foreach (IResourceLocation location in locations)
                        {
                            foreach (var dependency in location.Dependencies)
                            {
                                string remoteUrl = Addressables.ResourceManager.TransformInternalId(dependency);
                                if (DownloadContentConstants.IsDlcUrl(remoteUrl))
                                {
                                    StartUrlFetch(remoteUrl, onComplete);
                                }
                            }
                        }
                    }
                }
            }
        }

        private static void StartUrlFetch(string remoteUrl, Action onComplete)
        {
            _fetchCount++;
            // Simulate network request
            Task
                .Delay(1000)
                .ContinueWith(task =>
                {
                    if (task.IsCanceled || task.IsFaulted)
                    {
                        Debug.LogError($"Error fetching DLC url: {remoteUrl}");
                    }
                    else
                    {
                        // [TODO]: call backend to get presigned url
                        string url = remoteUrl.Replace(Addressables.RuntimePath, Addressables.BuildPath);
                        Debug.Log($"Fetched DLC url: {remoteUrl} => {url}");
                        DownloadContentController.SetInternalIdToDlcUrl(remoteUrl, url);

                        _fetchCount--;
                        if (_fetchCount == 0)
                        {
                            onComplete?.Invoke();
                        }
                    }
                });
        }
    }
}
