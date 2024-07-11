using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

public abstract class DownloadContentModelBase : ScriptableObject, IDisposable
{
    protected static Dictionary<string, AsyncOperationHandle> downloadingOperations = new Dictionary<string, AsyncOperationHandle>();
    public virtual void Dispose()
    {
        Debug.LogError("Don't forget to call UnloadDLC() to clear memory of all loaded assets");
    }

    #region Download Content Methods

    public static bool IsAssetLoaded<T>(AssetReference assetReference)
    where T : UnityEngine.Object
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

    public static T GetAsset<T>(AssetReferenceT<T> assetReference)
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


    public static void UnloadGroup(List<AssetReference> listAssetReference)
    {
        foreach (AssetReference assetReference in listAssetReference)
        {
            UnloadAsset(assetReference);
        }
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

    public static void DownloadGroup<T>(List<AssetReference> listAssetReference, Action<T> onDownloadedAsset)
    where T : UnityEngine.Object
    {
        foreach (AssetReference assetReference in listAssetReference)
        {
            DownloadAsset(assetReference, onDownloadedAsset);
        }
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
    public static float CurrentDownloadProgress()
    {
        float totalProgress = 0.0f;
        foreach (var handle in downloadingOperations.Values)
        {
            totalProgress += handle.PercentComplete;
        }
        return totalProgress / downloadingOperations.Count;
    }

    public static bool AllDownloadsCompleted()
    {
        foreach (var handle in downloadingOperations.Values)
        {
            if (!handle.IsDone)
            {
                return false;
            }
        }
        return true;
    }
    #endregion
}
