using DownloadContent.Helpers;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.ResourceManagement.ResourceLocations;
using Debug = UnityEngine.Debug;

namespace DownloadContent.Controllers
{
    public class DownloadContentController
    {
        protected readonly Dictionary<string, string> internalIdToDlcUrl = new Dictionary<string, string>();
        protected readonly Dictionary<string, string> originalDlcUrl = new Dictionary<string, string>();

        public virtual string IdTransformFunc(IResourceLocation location)
        {
            if (internalIdToDlcUrl.TryGetValue(location.InternalId, out string dlcUrl))
            {
                return dlcUrl;
            }

            return location.InternalId;
        }

        public virtual void SetInternalIdToDlcUrl(string internalId, string dlcUrl)
        {
            internalIdToDlcUrl[internalId] = dlcUrl;

            string sanitizedUrl = Uri.UnescapeDataString(dlcUrl);
            originalDlcUrl[sanitizedUrl] = dlcUrl;
        }

        public virtual void GetWebRequestFunc(UnityWebRequest request)
        {
            var originalUrl = GetOriginalDlcUrl(request.url);
            if (originalUrl != request.url)
            {
                Debug.Log($"Restore to original URL: {originalUrl}");
                request.url = originalUrl;
            }
        }

        private string GetOriginalDlcUrl(string url)
        {
            if (originalDlcUrl.TryGetValue(url, out string originalUrl))
            {
                return originalUrl;
            }

            return url;
        }
    }
}
