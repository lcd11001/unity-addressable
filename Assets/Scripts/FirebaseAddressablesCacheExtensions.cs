using System;
using System.Collections.Generic;
using Firebase.Extensions;
using Firebase.Storage;
using UnityEngine;
using UnityEngine.AddressableAssets.ResourceLocators;
using UnityEngine.ResourceManagement.ResourceLocations;

namespace RobinBird.FirebaseTools.Storage.Addressables
{
    // compare this snippet with FirebaseAddressablesCache.cs then copy and paste it to overwrite of FirebaseAddressablesCache.cs
    public static class FirebaseAddressablesCacheExtensions
    {
        private static readonly Dictionary<string, string> internalIdToStorageUrlDict = new Dictionary<string, string>();

        private static readonly Dictionary<string, string> originalStorageUrl = new Dictionary<string, string>();

        private static int runningFetchUrlOperationCount;


        public static string IdTransformFunc(IResourceLocation location)
        {
            if (internalIdToStorageUrlDict.TryGetValue(location.InternalId, out string storageUrl))
            {
                return storageUrl;
            }
            return location.InternalId;
        }

        public static string GetOriginalStorageUrl(string storageUrl)
        {
            if (originalStorageUrl.TryGetValue(storageUrl, out string originalUrl))
            {
                return originalUrl;
            }
            return storageUrl;
        }

        public static void SetInternalIdToStorageUrlMapping(string internalId, string storageUrl)
        {
            internalIdToStorageUrlDict[internalId] = storageUrl;

            string sanitizedUrl = Uri.UnescapeDataString(storageUrl);
            originalStorageUrl[sanitizedUrl] = storageUrl;
        }

        public static void PreWarmDependencies(object key, Action completed)
        {
            PreWarmDependencies(new List<object>() { key }, completed);
        }

        public static void PreWarmDependencies(List<object> keys, Action completed)
        {
            if (FirebaseAddressablesManager.IsFirebaseSetupFinished == false)
            {
                FirebaseAddressablesManager.FirebaseSetupFinished += () => { PreWarmDependencies(keys, completed); };
            }
            else
            {
                var initOperation = UnityEngine.AddressableAssets.Addressables.InitializeAsync();

                if (initOperation.IsDone == false)
                {
                    initOperation.Completed += handle => GetFirebaseUrl(keys, completed);
                }
                else
                {
                    GetFirebaseUrl(keys, completed);
                }
            }
        }

        private static void GetFirebaseUrl(List<object> keys, Action completed)
        {
            if (runningFetchUrlOperationCount > 0)
            {
                Debug.LogError("Wait until the previous operation is completed before starting a new one");
                return;
            }
            runningFetchUrlOperationCount = 0;
            foreach (var key in keys)
            {
                foreach (IResourceLocator locator in UnityEngine.AddressableAssets.Addressables.ResourceLocators)
                {
                    if (locator.Locate(key, typeof(object), out IList<IResourceLocation> locations))
                    {
                        foreach (IResourceLocation location in locations)
                        {
                            foreach (var dependency in location.Dependencies)
                            {
                                string firebaseUrl = UnityEngine.AddressableAssets.Addressables.ResourceManager.TransformInternalId(dependency);
                                if (FirebaseAddressablesManager.IsFirebaseStorageLocation(firebaseUrl) == false)
                                {
                                    continue;
                                }

                                Debug.Log("11111 " + firebaseUrl);
                                StorageReference reference = FirebaseStorage.DefaultInstance.GetReferenceFromUrl(firebaseUrl);

                                StartUrlFetch(completed, reference, firebaseUrl);
                            }
                        }
                    }
                }
            }
        }

        private static void StartUrlFetch(Action completed, StorageReference reference, string firebaseUrl)
        {
            runningFetchUrlOperationCount++;
            reference.GetDownloadUrlAsync().ContinueWithOnMainThread(task =>
            {
                if (task.IsCanceled || task.IsFaulted)
                {
                    Debug.LogError($"Could not get url for: {firebaseUrl}, {task.Exception}");
                }
                else
                {
                    string url = task.Result.ToString();
                    SetInternalIdToStorageUrlMapping(firebaseUrl, url);
                }

                runningFetchUrlOperationCount--;
                if (runningFetchUrlOperationCount <= 0)
                {
                    completed();
                }
            });
        }
    }
}