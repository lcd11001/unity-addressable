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

    public static bool IsAssetLoaded<T>(AssetReferenceT<T> assetReference)
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

    public static void UnloadDLC<T>(List<AssetReferenceT<T>> listAssetReference)
    where T : UnityEngine.Object
    {
        foreach (AssetReferenceT<T> assetReference in listAssetReference)
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
    }

    public static void DownloadDLC<T>(List<AssetReferenceT<T>> listAssetReference, Action<T> onDownloadedAsset)
    where T : UnityEngine.Object
    {
        foreach (AssetReferenceT<T> assetReference in listAssetReference)
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
            downloadingOperations.Add(assetReference.AssetGUID, handle);
        }
    }
}
