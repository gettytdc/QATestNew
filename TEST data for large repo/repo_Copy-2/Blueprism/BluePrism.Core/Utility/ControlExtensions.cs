namespace BluePrism.Core.Utility
{
    using System;
    using System.Windows.Forms;

    public static class ControlExtensions
    {
        public static void InvokeOnUiThread(this Control control, Action action)
        {
            if (!control.IsDisposedOrHandleIsMissing())
            {
                if (control.InvokeRequired)
                {
                    control.Invoke(new MethodInvoker(() =>
                        control.InvokeOnUiThread(action)));
                }
                else
                {
                    action();
                }
            }
        }

        public static void BeginInvokeOnUiThread(this Control control, Action action)
        {
            if (!control.IsDisposedOrHandleIsMissing())
            {
                if (control.InvokeRequired)
                {
                    control.BeginInvoke(new MethodInvoker(() =>
                        control.BeginInvokeOnUiThread(action)));
                }
                else
                {
                    action();
                }
            }
        }

        private static bool IsDisposedOrHandleIsMissing(this Control control)
            => control.IsDisposed || (!control.IsHandleCreated && !control.FindForm().IsHandleCreated);        
    }
}
