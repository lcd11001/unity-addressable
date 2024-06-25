using System;
using System.Collections.Generic;
using Firebase.Extensions;
using Firebase.Storage;
using UnityEngine;
using UnityEngine.AddressableAssets.ResourceLocators;
using UnityEngine.Networking;
using UnityEngine.ResourceManagement.ResourceLocations;

namespace RobinBird.FirebaseTools.Storage.Addressables
{
    public static class FirebaseAddressablesCacheExtensions
    {
        private static readonly Dictionary<string, string> originalStorageUrl = new Dictionary<string, string>();

        public static void GetWebRequestFunc(UnityWebRequest request)
        {
            var originalUrl = GetOriginalStorageUrl(request.url);
            if (originalUrl != request.url)
            {
                Debug.Log($"Retore to original URL: {originalUrl}");
                request.url = originalUrl;
            }
        }

        public static string IdTransformFunc(IResourceLocation location)
        {
            if (FirebaseAddressablesManager.IsFirebaseStorageLocation(location.InternalId))
            {
                GetFirebaseUrl(location.InternalId);
            }
            return FirebaseAddressablesCache.IdTransformFunc(location);
        }

        public static void GetFirebaseUrl(string gsUrl)
        {
            StorageReference reference = FirebaseStorage.DefaultInstance.GetReferenceFromUrl(gsUrl);
            reference.GetDownloadUrlAsync().ContinueWith(task =>
            {
                if (task.IsCanceled || task.IsFaulted)
                {
                    Debug.LogError($"Could not get url for: {gsUrl}, {task.Exception}");
                }
                else
                {
                    string firebaseUrl = task.Result.ToString();
                    string sanitizedUrl = Uri.UnescapeDataString(firebaseUrl);
                    originalStorageUrl[sanitizedUrl] = firebaseUrl;
                }
            });
        }

        public static string GetOriginalStorageUrl(string storageUrl)
        {
            if (originalStorageUrl.TryGetValue(storageUrl, out string originalUrl))
            {
                return originalUrl;
            }
            return storageUrl;
        }
    }
}