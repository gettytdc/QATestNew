using CefSharp;
using CefSharp.WinForms;

namespace BluePrism.ExternalLoginBrowser.Form
{
    public class BrowserFormFactory : IBrowserFormFactory
    {
        public IBrowserForm Create(IChromiumLoginBrowser browser)
        {
            InitializeChromiumEmbeddedFramework();
            return new BrowserForm(browser);
        }

        public static void InitializeChromiumEmbeddedFramework()
        {
            if (Cef.IsInitialized)
                return;

            var settings = new CefSettings
            {
                BrowserSubprocessPath = @"x86\CefSharp.BrowserSubprocess.exe"
            };

            Cef.Initialize(settings, false, null as IBrowserProcessHandler);
        }
    }
}