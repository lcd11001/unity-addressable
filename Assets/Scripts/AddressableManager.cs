using System;
using System.Collections;
using System.Collections.Generic;
using DownloadContent;
using DownloadContent.Services;
using DownloadContent.Views;
using RobinBird.FirebaseTools.Storage.Addressables;
using UnityEditor;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.AddressableAssets.ResourceLocators;
using UnityEngine.Networking;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.UI;

public class AddressableManager : DownloadContentViewBase
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

    [SerializeField]
    private Slider sliderProgress;

    private Coroutine smothSlider = null;

    protected override void Start()
    {
        base.Start();

        HideSlider();
        imageLogo.gameObject.SetActive(false);

        // For development purpose, clear cache
        Caching.ClearCache();
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();

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

    public override void OnDownloadContentSuccess()
    {
        Debug.Log("Addressables initialized successfully");

        DownloadContentModelBase.DownloadAsset<GameObject>(refCube, OnCubeLoaded);
        DownloadContentModelBase.DownloadAsset<Texture2D>(refLogo, OnLogoLoaded);
        DownloadContentModelBase.DownloadAsset<AudioClip>(refClip, OnClipLoaded);
        DownloadContentModelBase.DownloadAsset<GameObject>(refRotateCube, OnRotateCubeLoaded);

        StartCoroutine(DownloadProgress());

        AddressablesUtility.GetAddressFromAssetReference(refCube, (result) =>
        {
            Debug.Log($"Address of refCube: {result}");
        });
    }

    public override void OnDownloadContentFailed(Exception exception)
    {
        Debug.LogError(exception);
    }

    //private void Update()
    //{
    //    Debug.Log($"Download progress: {DownloadContentModelBase.CurrentDownloadProgress}");
    //}

    private void ShowSlider(float initValue = -1.0f)
    {
        if (sliderProgress != null)
        {
            if (initValue >= 0.0f)
            {
                sliderProgress.value = initValue;
            }

            if (sliderProgress.transform.parent != null)
            {
                sliderProgress.transform.parent.gameObject.SetActive(true);
            }
            else
            {
                sliderProgress.gameObject.SetActive(true);
            }
        }
    }

    private void HideSlider(float initValue = -1.0f)
    {
        if (sliderProgress != null)
        {
            if (initValue >= 0.0f)
            {
                sliderProgress.value = initValue;
            }

            if (sliderProgress.transform.parent != null)
            {
                sliderProgress.transform.parent.gameObject.SetActive(false);
            }
            else
            {
                sliderProgress.gameObject.SetActive(false);
            }
        }
    }

    private void UpdateSlider(float value)
    {
        if (sliderProgress != null)
        {
            //sliderProgress.value = value;
            if (value > sliderProgress.value && smothSlider != null)
            {
                StopCoroutine(smothSlider);
            }
            smothSlider = StartCoroutine(SmoothSliderUpdate(value));
        }
    }

    private IEnumerator SmoothSliderUpdate(float targetValue)
    {
        float currentValue = sliderProgress.value;
        float elapsedTime = 0f;
        float duration = 0.5f; // Duration in seconds over which the slider value changes

        while (elapsedTime < duration)
        {
            sliderProgress.value = Mathf.Lerp(currentValue, targetValue, elapsedTime / duration);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        sliderProgress.value = targetValue; // Ensure the target value is set
    }

    private IEnumerator DownloadProgress()
    {
        ShowSlider(0.0f);

        while (!DownloadContentModelBase.AllDownloadsCompleted)
        {
            float progress = DownloadContentModelBase.CurrentDownloadProgress;
            // Debug.Log($"Total progress: {progress}");
            UpdateSlider(progress);

            yield return null;
        }

        if (smothSlider != null)
        {
            UpdateSlider(1.0f);
            yield return new WaitUntil(() => sliderProgress.value == 1.0f);
        }

        // Debug.Log("All downloads completed.");
        HideSlider(1.0f);
    }

    private void OnRotateCubeLoaded(GameObject prefabCube)
    {
        var go = Instantiate(prefabCube);
        go.transform.position = cubePosition;

        var rotateCube = go.GetComponent<RotateCube>();
        rotateCube.SetSpeed(cubeRotationSpeed);
    }

    private void OnClipLoaded(AudioClip clip)
    {
        var go = new GameObject("Background Music");
        var audio = go.AddComponent<AudioSource>();
        audio.clip = clip;
        audio.loop = true;
        audio.playOnAwake = false;
        audio.Play();
    }

    private void OnLogoLoaded(Texture2D texture)
    {
        imageLogo.texture = texture;
        imageLogo.gameObject.SetActive(true);
    }

    private void OnCubeLoaded(GameObject prefab)
    {
        Instantiate(prefab);
    }
}
