using DownloadContent.Models;
using Firebase.Extensions;
using Firebase.Storage;
using UnityEngine;

[CreateAssetMenu(fileName = "FirestoreDownloadContentModel", menuName = "DLC/Content Downloaders/Firebase Firestore")]
public class FirestoreDownloadContentModel : ScriptableObject, IDownloadContentFetcher
{
    [Tooltip("Replace YOUR_PROJECT with your project id")]
    [SerializeField]
    private string GS_URL = "gs://YOUR_PROJECT.appspot.com/PATH_TO_ASSETS/";
    [SerializeField]
    private string FIRESTORE_URL = "firestore://";

    public bool IsSupportFormat(string url)
    {
        return !string.IsNullOrEmpty(url) && url.StartsWith("firestore://");
    }

    public void FetchUrl(string url, System.Action<string> onSuccess, System.Action<string> onError)
    {
        url = url.Replace(FIRESTORE_URL, GS_URL);
        var storage = FirebaseStorage.DefaultInstance;

        // Create a reference from a Google Cloud Storage URI
        var storageRef = storage.GetReferenceFromUrl(url);

        // Fetch the download URL
        storageRef.GetDownloadUrlAsync().ContinueWith(task =>
        {
            if (task.IsFaulted || task.IsCanceled)
            {
                onError?.Invoke($"Failed to fetch download URL {url}");
                return;
            }

            // Get the download URL
            string downloadUrl = task.Result.ToString();
            onSuccess?.Invoke(downloadUrl);
        });
    }
}

