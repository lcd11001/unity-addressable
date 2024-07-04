using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.AddressableAssets.ResourceLocators;
using UnityEngine.ResourceManagement.AsyncOperations;

public class AssetLoadTest : MonoBehaviour
{
    public AssetReferenceGameObject addressableAssetKey;
    public int numberInstances;

    private void Start()
    {
        // Load the asset multiple times
        Addressables.InitializeAsync().Completed += AssetLoadTest_Completed;
    }

    private void OnDestroy()
    {
        if (addressableAssetKey.IsValid())
        {
            addressableAssetKey.ReleaseAsset();
        }
    }

    private void AssetLoadTest_Completed(AsyncOperationHandle<IResourceLocator> handle)
    {
        if (handle.Status == AsyncOperationStatus.Succeeded)
        {
            LoadAssetMultipleTimes(addressableAssetKey, numberInstances);
        }
        else
        {
            Debug.LogError("Failed to initialize Addressables.");
        }
    }

    private void LoadAssetMultipleTimes(AssetReferenceGameObject key, int times)
    {
        for (int i = 0; i < times; i++)
        {
            // fix wrong callback index
            int localIndex = i;
            Addressables.LoadAssetAsync<GameObject>(key).Completed += (handle) => OnAssetLoaded(handle, localIndex);
        }
    }

    private void OnAssetLoaded(AsyncOperationHandle<GameObject> handle, int index)
    {
        if (handle.Status == AsyncOperationStatus.Succeeded)
        {
            GameObject asset = handle.Result;
            Debug.Log($"Asset loaded: {asset.name}");

            // Optionally, instantiate the asset
            Vector3 position = new Vector3(Random.Range(-5f, 5f), Random.Range(-5f, 5f), 0f);
            var go = Instantiate(asset, position, Quaternion.identity);
            go.name += $"_{index}";

            // Note: Consider releasing the asset if you're done with it, especially if you're not instantiating it
            // Addressables.Release(handle);
        }
        else
        {
            Debug.LogError("Failed to load asset.");
        }
    }
}
