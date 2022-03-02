using System;
using System.Windows.Forms;

namespace BluePrism.ExternalLoginBrowser
{
    public interface IBrowserForm : IDisposable
    {
        event LoginCompletedHandler LoginCompleted;
        event FormClosedEventHandler FormClosed;
        event LoginFailedHandler LoginFailed;

        void Show();

        void HideFormUntilLoaded();
    }
}
