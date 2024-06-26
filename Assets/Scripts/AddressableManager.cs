using System;
using System.Collections;
using System.Collections.Generic;
using RobinBird.FirebaseTools.Storage.Addressables;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.AddressableAssets.ResourceLocators;
using UnityEngine.Networking;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.UI;

public class AddressableManager : MonoBehaviour
{
    [SerializeField]
    private AssetReferenceGameObject refCube;

    [SerializeField]
    private AssetReferenceTexture2D refLogo;

    [SerializeField]
    private AssetReferenceAudioClip refClip;

    [SerializeField]
    private AssetReferenceRotateCube refRotateCube;

    [SerializeField]
    private RawImage imageLogo;

    [SerializeField]
    private Vector3 cubePosition = new Vector3(0, 0, 0);

    [SerializeField]
    private float cubeRotationSpeed = 1.0f;

    private Dictionary<string, float> downloadProgression = new Dictionary<string, float>();

    private void Start()
    {
        // For development purpose, clear cache
        Caching.ClearCache();

        // Hook Firebase
        Addressables.ResourceManager.ResourceProviders.Add(new FirebaseStorageAssetBundleProvider());
        Addressables.ResourceManager.ResourceProviders.Add(new FirebaseStorageJsonAssetProvider());
        Addressables.ResourceManager.ResourceProviders.Add(new FirebaseStorageHashProvider());

        //Addressables.InternalIdTransformFunc += FirebaseAddressablesCache.IdTransformFunc;
        Addressables.InternalIdTransformFunc += FirebaseAddressablesCacheExtensions.IdTransformFunc;
        Addressables.WebRequestOverride += FirebaseAddressablesCacheExtensions.GetWebRequestFunc;

        // Uncomment this line to see more logs
        //FirebaseAddressablesManager.LogLevel = Firebase.LogLevel.Verbose;
        FirebaseAddressablesManager.FirebaseSetupFinished += InitAddressable;

        // MUST be called after Firebase is initialized
        //Addressables.InitializeAsync().Completed += OnAddressablesInitialized;

        // refCube.LoadAssetAsync<GameObject>().Completed += OnCubeLoaded;
        // refLogo.LoadAssetAsync<Texture2D>().Completed += OnLogoLoaded;
        // refClip.LoadAssetAsync<AudioClip>().Completed += OnClipLoaded;

        /*
        FirebaseAddressablesCache.PreWarmDependencies(refCube.RuntimeKey, () =>
        {
            var handler = Addressables.GetDownloadSizeAsync(refCube.RuntimeKey);
            handler.Completed += (handle) =>
            {
                if (handle.Status == AsyncOperationStatus.Succeeded)
                {
                    Debug.Log($"Size of cube: {handle.Result}");
                }
                else
                {
                    Debug.LogError($"Failed to get size of cube: {handle.DebugName} due to {handle.OperationException}");
                }
            };
        });
        */
    }

    //private void Update()
    //{
    //    if (downloadProgression.Count > 0)
    //    {
    //        Debug.Log($"Total progress: {CalculateTotalProgress()}");
    //    }
    //}

    private float CalculateTotalProgress()
    {
        float totalProgress = 0.0f;
        foreach (var item in downloadProgression)
        {
            totalProgress += item.Value;
        }
        return totalProgress / downloadProgression.Count;
    }

    private IEnumerator TotalProgress()
    {
        float progress = 0.0f;
        while (progress < 1.0f)
        {
            Debug.Log($"Total progress: {progress}");
            progress = CalculateTotalProgress();
            yield return null;
        }
        Debug.Log($"Total progress: {progress}");
    }


    private IEnumerator DownloadStatus<T>(AsyncOperationHandle<T> handle)
    {
        if (downloadProgression.ContainsKey(handle.DebugName) == false)
        {
            downloadProgression.Add(handle.DebugName, 0.0f);
        }

        while (!handle.IsDone)
        {
            //Debug.Log($"{handle.DebugName}: {handle.PercentComplete}");
            downloadProgression[handle.DebugName] = handle.PercentComplete;
            yield return null;
        }

        downloadProgression[handle.DebugName] = 1.0f;
        //Debug.Log($"{handle.DebugName} is downloaded completed.");
    }

    private void OnAddressablesInitialized(AsyncOperationHandle<IResourceLocator> handle)
    {
        if (handle.Status == AsyncOperationStatus.Succeeded)
        {
            Debug.Log("Addressables initialized successfully");

            var handleCube = refCube.LoadAssetAsync<GameObject>();
            handleCube.Completed += OnCubeLoaded;
            StartCoroutine(DownloadStatus(handleCube));

            var handleLogo = refLogo.LoadAssetAsync<Texture2D>();
            handleLogo.Completed += OnLogoLoaded;
            StartCoroutine(DownloadStatus(handleLogo));

            var handleClip = refClip.LoadAssetAsync<AudioClip>();
            handleClip.Completed += OnClipLoaded;
            StartCoroutine(DownloadStatus(handleClip));

            var handleRotateCube = refRotateCube.InstantiateAsync(cubePosition, Quaternion.identity);
            handleRotateCube.Completed += OnRotateCubeLoaded;
            StartCoroutine(DownloadStatus(handleRotateCube));

            StartCoroutine(TotalProgress());

            AddressablesUtility.GetAddressFromAssetReference(refCube, (result) =>
            {
                Debug.Log($"Address of refCube: {result}");
            });
        }
        else
        {
            Debug.LogError("Addressables failed to initialize");
        }
    }

    private void OnRotateCubeLoaded(AsyncOperationHandle<RotateCube> handle)
    {
        DebugHandle(handle);

        if (handle.Status == AsyncOperationStatus.Succeeded)
        {
            handle.Result.SetSpeed(cubeRotationSpeed);
        }
    }

    private void OnDestroy()
    {
        if (refCube.IsValid())
        {
            refCube.ReleaseAsset();
        }
        if (refLogo.IsValid())
        {
            refLogo.ReleaseAsset();
        }
        if (refClip.IsValid())
        {
            refClip.ReleaseAsset();
        }
        if (refRotateCube.IsValid())
        {
            refRotateCube.ReleaseAsset();
        }
    }

    private static void DebugHandle<TObject>(AsyncOperationHandle<TObject> handle)
    {
        Debug.Log($"{handle.DebugName} is loaded {handle.Status}");
    }

    private void OnClipLoaded(AsyncOperationHandle<AudioClip> handle)
    {
        DebugHandle(handle);
        if (handle.Status == AsyncOperationStatus.Succeeded)
        {
            var go = new GameObject("Background Music");
            var audio = go.AddComponent<AudioSource>();
            audio.clip = handle.Result;
            audio.loop = true;
            audio.playOnAwake = false;
            audio.Play();
        }
    }

    private void OnLogoLoaded(AsyncOperationHandle<Texture2D> handle)
    {
        DebugHandle(handle);
        if (handle.Status == AsyncOperationStatus.Succeeded)
        {
            imageLogo.texture = handle.Result;
        }
    }

    private void OnCubeLoaded(AsyncOperationHandle<GameObject> handle)
    {
        DebugHandle(handle);
        if (handle.Status == AsyncOperationStatus.Succeeded)
        {
            Instantiate(handle.Result);
        }
    }

    public void OnFirebaseInitialized()
    {
        Debug.Log("Firebase initialized");
        FirebaseAddressablesManager.IsFirebaseSetupFinished = true;
    }

    public void InitAddressable()
    {
        Addressables.InitializeAsync().Completed += OnAddressablesInitialized;
    }
}
