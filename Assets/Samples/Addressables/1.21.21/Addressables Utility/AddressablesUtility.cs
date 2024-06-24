using System.Threading.Tasks;
using UnityEngine.AddressableAssets;
using UnityEngine.Events;
using UnityEngine.ResourceManagement.AsyncOperations;

/// <summary>
/// A utility class for various Addressables functionality
/// </summary>
public static class AddressablesUtility
{
    /// <summary>
    /// Get the address of a given AssetReference.
    /// </summary>
    /// <param name="reference">The AssetReference you want to find the address of.</param>
    /// <returns>The address of a given AssetReference.</returns>
    public static string GetAddressFromAssetReference(AssetReference reference)
    {
        var loadResourceLocations = Addressables.LoadResourceLocationsAsync(reference);
        var result = loadResourceLocations.WaitForCompletion();
        if (result.Count > 0)
        {
            string key = result[0].PrimaryKey;
            Addressables.Release(loadResourceLocations);
            return key;
        }

        Addressables.Release(loadResourceLocations);
        return string.Empty;
    }

    public static void GetAddressFromAssetReference(AssetReference reference, UnityAction<string> callback)
    {
        var loadResourceLocations = Addressables.LoadResourceLocationsAsync(reference);
        loadResourceLocations.Completed += handle =>
        {
            if (handle.Status == AsyncOperationStatus.Succeeded)
            {
                if (handle.Result.Count > 0)
                {
                    string key = handle.Result[0].PrimaryKey;
                    Addressables.Release(handle.Result);
                    callback(key);
                }
            }
            else
            {
                Addressables.Release(handle.Result);
                callback(string.Empty);
            }
        };
    }

    public static Task<string> GetAddressFromAssetReferenceAsync(AssetReference reference)
    {
        return Addressables.LoadResourceLocationsAsync(reference).Task.ContinueWith(result =>
        {
            if (result.Result.Count > 0)
            {
                string key = result.Result[0].PrimaryKey;
                Addressables.Release(result.Result);
                return key;
            }

            Addressables.Release(result.Result);
            return string.Empty;
        });
    }
}
