using System;
using System.Collections;
using System.Collections.Generic;
using Firebase.Storage;
using UnityEngine;
using UnityEngine.Events;

public class UploadScreenshot : MonoBehaviour
{
    public UploadEvent OnScreenshotUploaded;
    public void Upload(Texture2D screenshot)
    {
        Debug.Log("Uploading screenshot...");
        StartCoroutine(UploadScreenshotToFirebase(screenshot));
    }

    private IEnumerator UploadScreenshotToFirebase(Texture2D screenshot)
    {
        var storage = FirebaseStorage.DefaultInstance;
        var path = $"screenshots/{Guid.NewGuid()}.png";
        var reference = storage.GetReference(path);

        var bytes = screenshot.EncodeToPNG();

        var metadata = new MetadataChange();
        metadata.ContentType = "image/png";
        metadata.CustomMetadata = new Dictionary<string, string>(){
            {"timestamp", DateTime.UtcNow.ToString("dd/MM/yyyy HH:mm:ss")},
            {"resolution", $"{screenshot.width}x{screenshot.height}"},
            {"size", $"{bytes.Length} bytes"},
            {"width", screenshot.width.ToString()},
            {"height", screenshot.height.ToString()}
        };

        var handler = new UploadScreenshotHandler();

        var uploadTask = reference.PutBytesAsync(bytes, metadata, handler);
        yield return new WaitUntil(() => uploadTask.IsCompleted);

        if (uploadTask.Exception != null)
        {
            Debug.LogError($"Failed to upload screenshot with {uploadTask.Exception.Message}");
            yield break;
        }

        OnScreenshotUploaded?.Invoke(path);
    }
}

public class UploadScreenshotHandler : IProgress<UploadState>
{
    public void Report(UploadState state)
    {
        Debug.Log($"Upload Progress: {state.BytesTransferred}/{state.TotalByteCount}");
    }
}

[Serializable]
public class UploadEvent : UnityEvent<string>
{
}
