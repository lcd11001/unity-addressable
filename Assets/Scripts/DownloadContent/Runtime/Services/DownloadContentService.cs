using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using DownloadContent.Contants;
using DownloadContent.Controllers;
using DownloadContent.Views;
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

        private static Queue<Action> _fetchQueue = new Queue<Action>();
        private static bool _isFetching = false;

        public static void GetDlcUrlFromKey(object key, Action onComplete)
        {
            List<object> keys = new List<object> { key };
            GetDlcUrlFromKeys(keys, onComplete);
        }

        public static void GetDlcUrlFromKeys(List<object> keys, Action onComplete)
        {
            FetchUrlFromKeys(keys, onComplete);
        }

        public static void GetDlcUrlFromLocation(IResourceLocation location, Action onComplete)
        {
            List<IResourceLocation> locations = new List<IResourceLocation> { location };
            GetDlcUrlFromLocations(locations, onComplete);
        }

        public static void GetDlcUrlFromLocations(List<IResourceLocation> locations, Action onComplete)
        {
            List<object> keys = locations.Select(location => location.PrimaryKey as object).ToList();
            GetDlcUrlFromKeys(keys, onComplete);
        }

        public static void GetDlcUrlFromInternalId(string remoteUrl, Action onComplete)
        {
            _fetchQueue.Enqueue(() => FetchUrlFromRemote(remoteUrl, onComplete));
            if (!_isFetching)
            {
                ProcessQueue();
            }
        }

        public static void GetDlcUrlFromInternalId(IResourceLocation location, Action onComplete)
        {
            GetDlcUrlFromInternalId(location.InternalId, onComplete);
        }

        private static void FetchUrlFromRemotes(List<string> remoteUrls, Action onComplete)
        {
            foreach (var remoteUrl in remoteUrls)
            {
                if (DownloadContentConstants.IsDlcUrl(remoteUrl))
                {
                    StartUrlFetch(remoteUrl, onComplete);
                }
            }
        }

        private static void FetchUrlFromRemote(string remoteUrl, Action onComplete)
        {
            List<string> remoteUrls = new List<string> { remoteUrl };
            FetchUrlFromRemotes(remoteUrls, onComplete);
        }

        private static void ProcessQueue()
        {
            if (_fetchQueue.Count > 0)
            {
                _isFetching = true;
                Action fetchAction = _fetchQueue.Dequeue();
                fetchAction.Invoke();
            }
            else
            {
                _isFetching = false;
            }
        }

        public static void FetchUrlFromKeys(List<object> keys, Action onComplete)
        {
            /*
            List<string> remoteUrls = new List<string>();

            foreach (var key in keys)
            {
                foreach (IResourceLocator locator in Addressables.ResourceLocators)
                {
                    if (locator.Locate(key, typeof(object), out IList<IResourceLocation> locations))
                    {
                        foreach (IResourceLocation location in locations)
                        {
                            if (location.HasDependencies)
                            {
                                foreach (var dependency in location.Dependencies)
                                {
                                    remoteUrls.Add(dependency.InternalId);
                                }
                            }

                            remoteUrls.Add(location.InternalId);
                        }

                    }
                }
            }
            */
            List<string> remoteUrls = new List<string>();
            GetRemoteUrlsFromKeysRecursive(keys, remoteUrls);
            if (remoteUrls.Count == 0)
            {
                onComplete?.Invoke();
                return;
            }

            _fetchQueue.Enqueue(() => FetchUrlFromRemotes(remoteUrls, onComplete));
            if (!_isFetching)
            {
                ProcessQueue();
            }
        }

        private static void GetRemoteUrlsFromKeysRecursive(List<object> keys, List<string> remoteUrls)
        {
            if (keys == null || keys.Count == 0)
            {
                return;
            }

            foreach (var key in keys)
            {
                foreach (IResourceLocator locator in Addressables.ResourceLocators)
                {
                    if (locator.Locate(key, typeof(object), out IList<IResourceLocation> locations))
                    {
                        foreach (IResourceLocation location in locations)
                        {
                            if (location.HasDependencies)
                            {
                                List<object> subKeys = location.Dependencies.Select(dependency => dependency.PrimaryKey as object).ToList();
                                GetRemoteUrlsFromKeysRecursive(subKeys, remoteUrls);
                            }

                            remoteUrls.Add(location.InternalId);
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
                        string url = remoteUrl.Replace(DownloadContentConstants.DLC_URL_START, "DLC/");
                        Debug.Log($"Fetched DLC from {remoteUrl} => {url}");
                        DownloadContentController.SetInternalIdToDlcUrl(remoteUrl, url);

                        _fetchCount--;
                        if (_fetchCount == 0)
                        {
                            DownloadContentMainThread.ExecuteInMainThread(() => onComplete?.Invoke());
                            // onComplete?.Invoke();

                            // Continue with the next item in the queue
                            ProcessQueue();
                        }
                    }
                });
        }
    }
}
