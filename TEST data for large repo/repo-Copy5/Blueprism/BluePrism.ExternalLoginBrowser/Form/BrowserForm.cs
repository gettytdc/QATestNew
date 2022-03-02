using System;
using System.Windows.Forms;
using AutomateControls.Forms;
using BluePrism.Core.Utility;
using CefSharp;
using NLog;

namespace BluePrism.ExternalLoginBrowser.Form
{
    public partial class BrowserForm : ResizableBorderlessForm, IBrowserForm
    {
        private readonly IChromiumLoginBrowser _browser;
        public event LoginCompletedHandler LoginCompleted;
        public event LoginFailedHandler LoginFailed;

        private static readonly Logger Log = LogManager.GetCurrentClassLogger();
        private bool _hideFormUntilLoaded;

        internal BrowserForm(IChromiumLoginBrowser browser)
        {           
            InitializeComponent();
            
            NavigationMenu.FormCloseButtonClicked += HandleFormCloseButtonClicked;
            NavigationMenu.FormResizeButtonClicked += HandleFormResizeButtonClicked;
          
            _browser = browser;

            panBrowserFormBodyContainer.Controls.Add(_browser.WebBrowser);
            
            _browser.IsBrowserInitializedChanged += HandleIsBrowserInitializedChanged;
            _browser.LoadingStateChanged += HandleLoadingStateChanged;
            _browser.TitleChanged += HandleBrowserTitleChanged;
            _browser.LoadError += HandleBrowserLoadError;
            _browser.LoginCompleted += HandleLoginRequestCompleted;
            _browser.LoginFailed += HandleLoginRequestFailed;

            DragArea = panBrowserFormHeaderContainer;
        }

        public void HideFormUntilLoaded()
        {
            // hide the form until it has completely loaded
            Opacity = 0;
            ShowInTaskbar = false;
            _hideFormUntilLoaded = true;
        }

        private void ShowForm()
        {
            if (_hideFormUntilLoaded)
            {
                Opacity = 1;
                _hideFormUntilLoaded = false;
                BringToFront();
            }
        }

        private void HandleFormResizeButtonClicked(object sender, EventArgs e)
        {
            switch (WindowState)
            {
                case FormWindowState.Normal:
                    WindowState = FormWindowState.Maximized;
                    break;
                case FormWindowState.Maximized:
                    WindowState = FormWindowState.Normal;
                    break;
            }
        }

        private void HandleFormCloseButtonClicked(object sender, EventArgs e) 
            => Close();

       private void HandleIsBrowserInitializedChanged(object sender, IsBrowserInitializedChangedEventArgs e)
        {
            if (!e.IsBrowserInitialized) return;
            
            this.BeginInvokeOnUiThread(() 
                => _browser.Focus());
        }
        
        private void HandleLoadingStateChanged(object sender, LoadingStateChangedEventArgs args)
        {
            this.BeginInvokeOnUiThread(() =>
            {
                if (!args.IsLoading)
                {
                    ShowForm();
                }
            });

            this.BeginInvokeOnUiThread(()
                => _browser.UseWaitCursor = args.IsLoading);
        }

        private void HandleLoginRequestCompleted(object sender, LoginCompletedEventArgs e)
            => LoginCompleted?.Invoke(this, e);

        private void HandleLoginRequestFailed(object sender, LoginFailedEventArgs e)
            => LoginFailed?.Invoke(this, e);

        private void HandleBrowserTitleChanged(object sender, TitleChangedEventArgs args)
            => this.BeginInvokeOnUiThread(() => Text = args.Title);

        private void HandleBrowserLoadError(object sender, LoadErrorEventArgs e)
        {
            if (e.ErrorCode == CefErrorCode.Aborted)
                return;

            Log.Error("CefSharp encountered an error: Error Code {0}, {1}",
                e.ErrorCode, e.ErrorText);
        }
    }
}