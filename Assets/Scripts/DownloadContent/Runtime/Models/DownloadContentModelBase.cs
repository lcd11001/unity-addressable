using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace DownloadContent.Models
{
    public interface IDownloadContentFetcher
    {
        bool IsSupportFormat(string url);
        void FetchUrl(string url, Action<string> onSuccess, Action<string> onError);
    }

    public abstract class DownloadContentModelBase : ScriptableObject, IDisposable
    {
        protected static Dictionary<string, AsyncOperationHandle> downloadingOperations = new Dictionary<string, AsyncOperationHandle>();

        // Don't forget to call UnloadDLC() to clear memory of all loaded assets
        public abstract void Dispose();

        #region Assets Methods
        public static bool IsAssetLoaded(AssetReference assetReference)
        {
            if (assetReference == null)
            {
                Debug.LogError("AssetReference is null");
                return false;
            }

            if (downloadingOperations.ContainsKey(assetReference.AssetGUID))
            {
                var handle = downloadingOperations[assetReference.AssetGUID];
                return handle.IsValid() && handle.Status == AsyncOperationStatus.Succeeded;
            }
            return false;
        }

        public static T GetAsset<T>(AssetReference assetReference)
        where T : UnityEngine.Object
        {
            if (assetReference == null)
            {
                Debug.LogError("AssetReference is null");
                return default(T);
            }

            if (downloadingOperations.ContainsKey(assetReference.AssetGUID))
            {
                var handle = downloadingOperations[assetReference.AssetGUID];
                if (handle.IsValid() && handle.Status == AsyncOperationStatus.Succeeded)
                {
                    return handle.Result as T;
                }
            }
            return default(T);
        }
        #endregion

        #region Unloadd Content Methods
        public static void UnloadGroup(List<AssetReference> listAssetReference)
        {
            foreach (AssetReference assetReference in listAssetReference)
            {
                UnloadAsset(assetReference);
            }
        }

        public static void UnloadGroup<T>(List<AssetReferenceT<T>> listAssetReference)
        where T : UnityEngine.Object
        {
            foreach (AssetReferenceT<T> assetReference in listAssetReference)
            {
                UnloadAsset((AssetReference)assetReference);
            }
        }

        public static void UnloadAsset<T>(AssetReferenceT<T> assetReference)
        where T : UnityEngine.Object
        {
            UnloadAsset((AssetReference)assetReference);
        }

        public static void UnloadAsset(AssetReference assetReference)
        {
            if (downloadingOperations.ContainsKey(assetReference.AssetGUID))
            {
                var handle = downloadingOperations[assetReference.AssetGUID];
                if (handle.IsValid())
                {
                    Addressables.Release(handle);
                }
                downloadingOperations.Remove(assetReference.AssetGUID);
            }
        }
        #endregion

        #region Download Content Methods
        public static void DownloadGroup<T>(List<AssetReference> listAssetReference, Action<T> onDownloadedAsset)
        where T : UnityEngine.Object
        {
            foreach (AssetReference assetReference in listAssetReference)
            {
                DownloadAsset(assetReference, onDownloadedAsset);
            }
        }

        public static void DownloadGroup<T>(List<AssetReferenceT<T>> listAssetReference, Action<T> onDownloadedAsset)
        where T : UnityEngine.Object
        {
            foreach (AssetReferenceT<T> assetReference in listAssetReference)
            {
                DownloadAsset((AssetReference)assetReference, onDownloadedAsset);
            }
        }

        public static void DownloadAsset<T>(AssetReferenceT<T> assetReference, Action<T> onDownloadedAsset)
        where T : UnityEngine.Object
        {
            DownloadAsset((AssetReference)assetReference, onDownloadedAsset);
        }

        public static void DownloadAsset<T>(AssetReference assetReference, Action<T> onDownloadedAsset)
        where T : UnityEngine.Object
        {
            var handle = Addressables.LoadAssetAsync<T>(assetReference);
            handle.Completed += (obj) =>
            {
                if (obj.Status == AsyncOperationStatus.Succeeded)
                {
                    Debug.Log($"Downloaded [{typeof(T)}]: {obj.Result.name}");
                    onDownloadedAsset?.Invoke(obj.Result);
                }
                else
                {
                    Debug.LogError($"Failed to download [{typeof(T)}]: error {obj.OperationException}");
                }
            };

            if (downloadingOperations.ContainsKey(assetReference.AssetGUID))
            {
                downloadingOperations[assetReference.AssetGUID] = handle;
            }
            else
            {
                downloadingOperations.Add(assetReference.AssetGUID, handle);
            }
        }

        #endregion

        #region Downloading Progress
        public static bool IsDownloading
        {
            get
            {
                return downloadingOperations.Count > 0;
            }
        }

        public static float CurrentDownloadProgress
        {
            get
            {
                if (IsDownloading == false)
                {
                    return 0f;
                }

                float totalProgress = 0.0f;
                foreach (var handle in downloadingOperations.Values)
                {
                    totalProgress += handle.PercentComplete;
                }
                return totalProgress / downloadingOperations.Count;
            }
        }

        public static bool AllDownloadsCompleted
        {
            get
            {
                if (IsDownloading == false)
                {
                    return false;
                }

                foreach (var handle in downloadingOperations.Values)
                {
                    if (!handle.IsDone)
                    {
                        return false;
                    }
                }
                return true;
            }
        }
        #endregion
    }
}
