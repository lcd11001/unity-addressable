using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

namespace DownloadContent.Models
{
    public interface IDownloadContentFetcher
    {
        bool IsSupportFormat(string url);
        void FetchUrl(string url, Action<string> onSuccess, Action<string> onError);
    }

    public class DefaultDownloadContentModel : IDownloadContentFetcher
    {
        const string DLC_URL_START = "dlc://";
        const string LOCAL_URL_START = "DLC/";

        public bool IsSupportFormat(string url)
        {
            return !string.IsNullOrEmpty(url) && url.StartsWith(DLC_URL_START);
        }

        public void FetchUrl(string url, Action<string> onSuccess, Action<string> onError)
        {
            // Simulate network request
            Task
                .Delay(1000)
                .ContinueWith(task =>
                {
                    if (task.IsCanceled || task.IsFaulted)
                    {
                        onError?.Invoke("Error");
                    }
                    else
                    {
                        string dlcUrl = url.Replace(DLC_URL_START, LOCAL_URL_START);
                        onSuccess?.Invoke(dlcUrl);
                    }
                });
        }
    }
}
