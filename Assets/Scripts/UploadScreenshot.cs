using System;
using System.Collections;
using System.Collections.Generic;
using Firebase.Storage;
using UnityEngine;

public class UploadScreenshot : MonoBehaviour
{
    public void Upload(Texture2D screenshot)
    {
        StartCoroutine(UploadScreenshotToFirebase(screenshot));
    }

    private IEnumerator UploadScreenshotToFirebase(Texture2D screenshot)
    {
        var storage = FirebaseStorage.DefaultInstance;
        var reference = storage.GetReference($"screenshots/{Guid.NewGuid()}.png");

        var bytes = screenshot.EncodeToPNG();

        var metadata = new MetadataChange();
        metadata.ContentType = "image/png";
        metadata.CustomMetadata = new Dictionary<string, string>(){
            {"timestamp", DateTime.UtcNow.ToString("dd/MM/yyyy HH:mm:ss")}
        };

        var handler = new UploadScreenshotHandler();

        var uploadTask = reference.PutBytesAsync(bytes, metadata, handler);
        yield return new WaitUntil(() => uploadTask.IsCompleted);

        if (uploadTask.Exception != null)
        {
            Debug.LogError($"Failed to upload screenshot with {uploadTask.Exception.Message}");
            yield break;
        }

        var getUrlTask = reference.GetDownloadUrlAsync();
        yield return new WaitUntil(() => getUrlTask.IsCompleted);

        if (getUrlTask.Exception != null)
        {
            Debug.LogError($"Failed to get download URL with {getUrlTask.Exception.Message}");
            yield break;
        }

        var url = getUrlTask.Result.ToString();
        Debug.Log($"Screenshot uploaded to {url}");
    }
}

public class UploadScreenshotHandler : IProgress<UploadState>
{
    public void Report(UploadState state)
    {
        Debug.Log($"Progress: {state.BytesTransferred}/{state.TotalByteCount}");
    }
}
