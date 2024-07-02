using DLC.Contants;
using DLC.Services;
using DownloadContent.Controllers;
using DownloadContent.Helpers;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Networking;
using UnityEngine.ResourceManagement.ResourceLocations;

namespace DownloadContent
{
    public class DownloadContentManager : Singleton<DownloadContentManager>
    {
        private bool _isInitialized = false;
        public bool IsInitialized => _isInitialized;

        public event Action OnInitialized;

        private Func<IResourceLocation, string> _internalIdTransformFunc;
        protected Func<IResourceLocation, string> InternalIdTransformFunc
        {
            get => _internalIdTransformFunc;
            private set
            {
                _internalIdTransformFunc = value;
                Addressables.ResourceManager.InternalIdTransformFunc = value;
            }
        }

        private Action<UnityWebRequest> _webRequestOverride;
        protected Action<UnityWebRequest> WebRequestOverride
        {
            get => _webRequestOverride;
            private set
            {
                _webRequestOverride = value;
                Addressables.ResourceManager.WebRequestOverride = value;
            }
        }

        private DownloadContentController _controller;
        private DownloadContentService _service;

        public DownloadContentManager()
        {
            _controller = new DownloadContentController();
            _service = new DownloadContentService();
        }

        public void Initialize()
        {
            Initialize(_controller.IdTransformFunc, _controller.GetWebRequestFunc);
        }

        public void Initialize(Func<IResourceLocation, string> internalIdTransformFunc, Action<UnityWebRequest> webRequestOverride)
        {
            if (_isInitialized)
            {
                return;
            }

            InternalIdTransformFunc = internalIdTransformFunc;
            WebRequestOverride = webRequestOverride;

            _isInitialized = true;
            OnInitialized?.Invoke();
        }

        public bool IsDlcUrl(string url)
        {
            return !string.IsNullOrEmpty(url) && url.StartsWith(DownloadContentConstants.DLC_URL_START);
        }

        public void CacheDlcUrl(string remoteUrl, string dlcUrl)
        {
            _controller.SetInternalIdToDlcUrl(remoteUrl, dlcUrl);
        }
    }
}
