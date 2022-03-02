using System.Windows.Forms;
using System.Reflection;
using AutomateControls.WindowsSupport;

namespace AutomateControls
{
    /// <summary>
    /// A double buffered split container which ensures that it and its 2 child
    /// panels are buffered and WM_ERASEBKGND messages are ignored
    /// </summary>
    public class FlickerFreeSplitContainer: SplitContainer
    {
        /// <summary>
        /// Creates a new empty flicker-free SplitContainer
        /// </summary>
        public FlickerFreeSplitContainer()
        {
            // Set the control styles we need.
            // EnableNotifyMessage ensures that we get a chance to filter out Windows
            // messages before they get to WndProc
            SetStyle(
                ControlStyles.OptimizedDoubleBuffer
                | ControlStyles.AllPaintingInWmPaint
                | ControlStyles.EnableNotifyMessage, true);

            MethodInfo methInfo = typeof(Control).GetMethod(
                "SetStyle", BindingFlags.NonPublic | BindingFlags.Instance);

            object[] args = {
                ControlStyles.AllPaintingInWmPaint |
                ControlStyles.UserPaint |
                ControlStyles.OptimizedDoubleBuffer, true
            };

            methInfo.Invoke(this.Panel1, args);
            methInfo.Invoke(this.Panel2, args);
        }

        /// <summary>
        /// Filter out the 'Erase Background' message.
        /// </summary>
        /// <param name="m">The message this object is being notified of.</param>
        protected override void OnNotifyMessage(Message m)
        {
            if (m.Msg != WindowsMessage.WM_ERASEBKGND)
                base.OnNotifyMessage(m);
        }
    }
}
