using System;
using System.Diagnostics;

namespace BluePrism.Core.Utility
{
    public class ExternalBrowser
    {
        public static void OpenUrl(string url)
        {
            OpenUrl(new Uri(url));
        }

        public static void OpenUrl(Uri url)
        {
            Process.Start(url.ToString());
        }
    }
}
