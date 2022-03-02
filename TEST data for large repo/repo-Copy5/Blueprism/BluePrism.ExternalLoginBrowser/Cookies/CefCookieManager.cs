using System;
using CefSharp;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BluePrism.Common.Security;

namespace BluePrism.ExternalLoginBrowser.Cookies
{
    public class CefCookieManager : ICookieManager
    {
        private async Task DeleteAllCookiesFromBrowser()
        {
            var cookieManager = GetCookieManager();
            await cookieManager.DeleteCookiesAsync();
        }

        public async Task DeleteIdentityCookieFromBrowser()
        {
            await DeleteAllCookiesFromBrowser();
        }

        private static CefSharp.ICookieManager GetCookieManager()
        {
            var cookieManager = Cef.GetGlobalCookieManager();

            if (cookieManager == null)
            {
                throw new InvalidOperationException("No Cef cookie manager available");
            }

            return cookieManager;
        }
    }
}
