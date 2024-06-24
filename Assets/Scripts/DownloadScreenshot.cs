using System;
using System.Collections;
using System.Collections.Generic;
using Firebase.Storage;
using UnityEngine;

public class DownloadScreenshot : MonoBehaviour
{
    public ScreenshotEvent OnScreenshotDownloaded;

    public void Download(string screenshotPath)
    {
        Debug.Log($"Downloading screenshot from {screenshotPath}");
        StartCoroutine(DownloadScreenshotCoroutine(screenshotPath));
    }

    private IEnumerator DownloadScreenshotCoroutine(string screenshotPath)
    {
        var storage = FirebaseStorage.DefaultInstance;
        var reference = storage.GetReference(screenshotPath);

        var handler = new DownloadHandler();
        var downloadTask = reference.GetBytesAsync(long.MaxValue, handler);
        yield return new WaitUntil(() => downloadTask.IsCompleted);

        if (downloadTask.Exception != null)
        {
            Debug.LogError($"Failed to download screenshot with {downloadTask.Exception.Message}");
            yield break;
        }

        var metadataTask = reference.GetMetadataAsync();
        yield return new WaitUntil(() => metadataTask.IsCompleted);

        if (metadataTask.Exception != null)
        {
            Debug.LogError($"Failed to get metadata with {metadataTask.Exception.Message}");
            yield break;
        }

        byte[] bytes = downloadTask.Result;
        StorageMetadata metadata = metadataTask.Result;

        int width = int.Parse(metadata.GetCustomMetadata("width") ?? "1024");
        int height = int.Parse(metadata.GetCustomMetadata("height") ?? "1024");

        Debug.Log($"Downloaded screenshot with {width}x{height}");
        var texture = new Texture2D(width, height);
        texture.LoadImage(bytes);

        OnScreenshotDownloaded?.Invoke(texture);
    }
}

public class DownloadHandler : IProgress<DownloadState>
{
    public void Report(DownloadState state)
    {
        Debug.Log($"Download Progress: {state.BytesTransferred}/{state.TotalByteCount}");
    }
}
