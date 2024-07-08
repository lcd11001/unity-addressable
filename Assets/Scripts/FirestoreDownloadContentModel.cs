using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using DownloadContent.Models;
using Firebase.Extensions;
using UnityEngine;

public class FirestoreDownloadContentModel : IDownloadContentFetcher
{
    public bool IsSupportFormat(string url)
    {
        return !string.IsNullOrEmpty(url) && url.StartsWith("firestore://");
    }

    public void FetchUrl(string url, System.Action<string> onSuccess, System.Action<string> onError)
    {
        // Simulate network request
        Task
            .Delay(1000)
            .ContinueWithOnMainThread(task =>
            {
                if (false && Random.value < 0.5f)
                {
                    onError?.Invoke("Error");
                }
                else
                {
                    string firestoreUrl = url.Replace("firestore://", "DLC_FIREBASE/");
                    onSuccess?.Invoke(firestoreUrl);
                }
            });
    }
}

