using System;
using System.Collections.Generic;
using System.Linq;
using DownloadContent.Controllers;
using DownloadContent.Models;
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
        #region Download Content Fetcher

        private static List<IDownloadContentFetcher> downloadContentFetchers = new List<IDownloadContentFetcher>
        {
            new DefaultDownloadContentModel()
        };

        public static void AddDownloadContentFetcher(IDownloadContentFetcher fetcher)
        {
            Type type = GetLowestType(fetcher.GetType());
            if (IsContainsType(downloadContentFetchers, type))
            {
                return;
            }
            downloadContentFetchers.Add(fetcher);
        }

        public static void InsertDownloadContentFetcher(IDownloadContentFetcher fetcher, int index)
        {
            Type type = GetLowestType(fetcher.GetType());
            if (IsContainsType(downloadContentFetchers, type))
            {
                return;
            }
            downloadContentFetchers.Insert(index, fetcher);
        }

        public static void RemoveDownloadContentFetcher(IDownloadContentFetcher fetcher)
        {
            downloadContentFetchers.Remove(fetcher);
        }

        public static void RemoveDownloadContentFetcherAt(int index)
        {
            downloadContentFetchers.RemoveAt(index);
        }

        public static IDownloadContentFetcher GetDownloadContentFetcher(string url)
        {
            foreach (var fetcher in downloadContentFetchers)
            {
                if (fetcher.IsSupportFormat(url))
                {
                    return fetcher;
                }
            }

            return null;
        }

        public static bool IsSupportFormat(string url)
        {
            foreach (var fetcher in downloadContentFetchers)
            {
                if (fetcher.IsSupportFormat(url))
                {
                    return true;
                }
            }
            return false;
        }

        #endregion

        #region Download Queue

        private static int _fetchCount = 0;
        private static Queue<Action> _fetchQueue = new Queue<Action>();
        private static bool _isFetching = false;

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

        private static void CheckAndProcessQueue(Action onComplete)
        {
            _fetchCount--;
            if (_fetchCount == 0)
            {
                DownloadContentMainThread.ExecuteInMainThread(() => onComplete?.Invoke());
                // onComplete?.Invoke();

                // Continue with the next item in the queue
                ProcessQueue();
            }
        }

        #endregion

        #region Get DLC Url

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

        #endregion

        #region Fetch DLC Url

        private static void FetchUrlFromRemotes(List<string> remoteUrls, Action onComplete)
        {
            foreach (var remoteUrl in remoteUrls)
            {
                StartUrlFetch(remoteUrl, onComplete);
            }
        }

        private static void FetchUrlFromRemote(string remoteUrl, Action onComplete)
        {
            List<string> remoteUrls = new List<string> { remoteUrl };
            FetchUrlFromRemotes(remoteUrls, onComplete);
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

            IDownloadContentFetcher fetcher = GetDownloadContentFetcher(remoteUrl);
            if (fetcher != null)
            {
                fetcher.FetchUrl(remoteUrl,
                    (dlcUrl) =>
                    {
                        Debug.Log($"Fetched DLC from {remoteUrl} => {dlcUrl}");
                        // set for caching the dlc url
                        DownloadContentController.SetInternalIdToDlcUrl(remoteUrl, dlcUrl);
                        CheckAndProcessQueue(onComplete);
                    },
                    (error) =>
                    {
                        Debug.LogError($"Error fetching DLC url: {remoteUrl} \nError Message: {error}");
                        // clear the cache
                        DownloadContentController.RemoveInteralId(remoteUrl);
                        CheckAndProcessQueue(onComplete);
                    }
                );
            }
            else
            {
                Debug.LogError($"No fetcher found for {remoteUrl}");
                CheckAndProcessQueue(onComplete);
            }
        }

        #endregion

        #region Utility
        public static Type GetLowestType(Type type)
        {
            return GetTypeHierarchy(type).LastOrDefault();
        }

        private static Type[] GetTypeHierarchy(Type type)
        {
            var hierarchy = new System.Collections.Generic.List<Type>();

            while (type != null)
            {
                hierarchy.Insert(0, type);  // Insert at the beginning to maintain order
                type = type.BaseType;
            }

            return hierarchy.ToArray();
        }

        public static bool IsContainsType<T>(IList<T> list, Type type)
        {
            if (list.Any(f => GetLowestType(f.GetType()) == type))
            {
                Debug.LogWarning($"{type} already exists in list {nameof(list)}");
                return true;
            }
            return false;
        }

        #endregion
    }
}
