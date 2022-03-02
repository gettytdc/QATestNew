using CefSharp.WinForms;
using System.Windows.Forms;

namespace BluePrism.ExternalLoginBrowser
{
    public class ChromiumLoginBrowserFactory : IChromiumLoginBrowserFactory
    {
        public IChromiumLoginBrowser Create(string startUrl, string endUrl)
        {
            var webBrowser = new ChromiumWebBrowser(startUrl)
            {
                Dock = DockStyle.Fill,
                Margin = Padding.Empty,
                Padding = Padding.Empty
            };
            webBrowser.MenuHandler = new BrowserContextMenuHandler();

            var requestHandler = new ExternalLoginRequestHandler(endUrl);
            return new ChromiumLoginBrowser(webBrowser, requestHandler);
        }
    }
}
