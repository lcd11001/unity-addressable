using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.AddressableAssets.ResourceLocators;
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
    private RawImage imageLogo;

    private void Start()
    {
        Addressables.InitializeAsync().Completed += OnAddressablesInitialized;

        // refCube.LoadAssetAsync<GameObject>().Completed += OnCubeLoaded;
        // refLogo.LoadAssetAsync<Texture2D>().Completed += OnLogoLoaded;
        // refClip.LoadAssetAsync<AudioClip>().Completed += OnClipLoaded;
    }

    private void OnAddressablesInitialized(AsyncOperationHandle<IResourceLocator> handle)
    {
        if (handle.Status == AsyncOperationStatus.Succeeded)
        {
            Debug.Log("Addressables initialized successfully");
            refCube.LoadAssetAsync<GameObject>().Completed += OnCubeLoaded;
            refLogo.LoadAssetAsync<Texture2D>().Completed += OnLogoLoaded;
            refClip.LoadAssetAsync<AudioClip>().Completed += OnClipLoaded;

            // AddressablesUtility.GetAddressFromAssetReference(refCube, (result) =>
            // {
            //     Debug.Log($"Address of refCube: {result}");
            // });
        }
        else
        {
            Debug.LogError("Addressables failed to initialize");
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
}
