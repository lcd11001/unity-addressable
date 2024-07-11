using System;
using System.Collections;
using System.Collections.Generic;
using DownloadContent.Controllers;
using DownloadContent.Models;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.ResourceManagement.ResourceLocations;

namespace DownloadContent.Services
{
    public abstract class DownloadContentServiceBase : ScriptableObject, IDownloadContentFetcher, IDisposable
    {
        public Action onDownloadContentServicedInitialized;
        private bool _isInitialized = false;
        public bool IsInitialized
        {
            get => _isInitialized;
            set
            {
                _isInitialized = value;
                if (_isInitialized)
                {
                    onDownloadContentServicedInitialized?.Invoke();
                }
            }
        }
        public abstract void FetchUrl(string url, Action<string> onSuccess, Action<string> onError);
        public abstract bool IsSupportFormat(string url);

        public virtual void Initialize()
        {
            IsInitialized = true;
        }

        public virtual string IdTransformFunc(IResourceLocation location)
        {
            return DownloadContentController.IdTransformFunc(location);
        }

        public virtual void GetWebRequestFunc(UnityWebRequest request)
        {
            DownloadContentController.GetWebRequestFunc(request);
        }

        public virtual void Dispose()
        {
            _isInitialized = false;
        }
    }
}
