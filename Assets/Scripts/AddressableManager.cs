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

    private Dictionary<string, float> downloadProgression = new Dictionary<string, float>();
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

    public override void OnDownloadContentFailed(Exception exception)
    {
        Debug.LogError(exception);
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

    private bool AllDownloadsCompleted()
    {
        foreach (var item in downloadProgression)
        {
            if (item.Value < 1.0f) return false;
        }
        return true;
    }

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

    private IEnumerator TotalProgress()
    {
        ShowSlider(0.0f);

        while (!AllDownloadsCompleted())
        {
            float progress = CalculateTotalProgress();
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

    private void OnRotateCubeLoaded(AsyncOperationHandle<RotateCube> handle)
    {
        DebugHandle(handle);

        if (handle.Status == AsyncOperationStatus.Succeeded)
        {
            // handle.Result.SetSpeed(cubeRotationSpeed);
            StartCoroutine(ShowRotateCube(handle.Result));
        }
    }

    private IEnumerator ShowRotateCube(RotateCube cube)
    {
        cube.gameObject.SetActive(false);
        while (AllDownloadsCompleted() == false)
        {
            yield return null;
        }
        cube.gameObject.SetActive(true);
        cube.SetSpeed(cubeRotationSpeed);
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

            StartCoroutine(PlayMusic(audio));
            // audio.Play();
        }
    }

    private IEnumerator PlayMusic(AudioSource audio)
    {
        while (AllDownloadsCompleted() == false)
        {
            yield return null;
        }
        if (audio.clip.loadState == AudioDataLoadState.Loading)
        {
            yield return new WaitUntil(() => audio.clip.loadState == AudioDataLoadState.Loaded);
        }
        audio.Play();
    }

    private void OnLogoLoaded(AsyncOperationHandle<Texture2D> handle)
    {
        DebugHandle(handle);
        if (handle.Status == AsyncOperationStatus.Succeeded)
        {
            // imageLogo.texture = handle.Result;
            StartCoroutine(ShowLogo(handle.Result));
        }
    }

    private IEnumerator ShowLogo(Texture2D texture)
    {
        while (AllDownloadsCompleted() == false)
        {
            yield return null;
        }
        imageLogo.texture = texture;
        imageLogo.gameObject.SetActive(true);
    }

    private void OnCubeLoaded(AsyncOperationHandle<GameObject> handle)
    {
        DebugHandle(handle);
        if (handle.Status == AsyncOperationStatus.Succeeded)
        {
            // Instantiate(handle.Result);
            StartCoroutine(ShowCube(handle.Result));
        }
    }

    private IEnumerator ShowCube(GameObject prefab)
    {
        while (AllDownloadsCompleted() == false)
        {
            yield return null;
        }
        Instantiate(prefab);
    }
}
