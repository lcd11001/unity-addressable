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

    private void Start()
    {
        Caching.ClearCache();

        // Hook Firebase
        Addressables.ResourceManager.ResourceProviders.Add(new FirebaseStorageAssetBundleProvider());
        Addressables.ResourceManager.ResourceProviders.Add(new FirebaseStorageJsonAssetProvider());
        Addressables.ResourceManager.ResourceProviders.Add(new FirebaseStorageHashProvider());

        Addressables.InternalIdTransformFunc += FirebaseAddressablesCache.IdTransformFunc;
        Addressables.WebRequestOverride += GetWebRequest;


        FirebaseAddressablesManager.LogLevel = Firebase.LogLevel.Verbose;
        FirebaseAddressablesManager.FirebaseSetupFinished += InitAddressable;

        // MUST be called after Firebase is initialized
        //Addressables.InitializeAsync().Completed += OnAddressablesInitialized;

        // refCube.LoadAssetAsync<GameObject>().Completed += OnCubeLoaded;
        // refLogo.LoadAssetAsync<Texture2D>().Completed += OnLogoLoaded;
        // refClip.LoadAssetAsync<AudioClip>().Completed += OnClipLoaded;
    }

    public static void GetWebRequest(UnityWebRequest request)
    {
        Debug.Log($"Requesting {request.url}");

        var originalUrl = FirebaseAddressablesCache.GetOriginalStorageUrl(request.url);
        Debug.Log($"Original URL: {originalUrl}");
        request.url = originalUrl; // Replace the URL with the original URL
    }

    private void OnAddressablesInitialized(AsyncOperationHandle<IResourceLocator> handle)
    {
        if (handle.Status == AsyncOperationStatus.Succeeded)
        {
            Debug.Log("Addressables initialized successfully");

            refCube.LoadAssetAsync<GameObject>().Completed += OnCubeLoaded;
            refLogo.LoadAssetAsync<Texture2D>().Completed += OnLogoLoaded;
            refClip.LoadAssetAsync<AudioClip>().Completed += OnClipLoaded;

            refRotateCube.InstantiateAsync(cubePosition, Quaternion.identity).Completed += OnRotateCubeLoaded;


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
