using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DownloadContent.Contants
{
    public static class DownloadContentConstants
    {
        public const string DLC_URL_START = "dlc://";

        public static bool IsDlcUrl(string url)
        {
            return !string.IsNullOrEmpty(url) && url.StartsWith(DLC_URL_START);
        }
    }
}
