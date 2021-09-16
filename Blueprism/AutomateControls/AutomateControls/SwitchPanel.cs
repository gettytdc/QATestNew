using System.Windows.Forms;
using AutomateControls.WindowsSupport;
using System.ComponentModel;

namespace AutomateControls
{
    public class SwitchPanel : TabControl
    {
        // Flag indicating if the tabs are visible or not
        private bool _tabsVisible, _disableArrowKeys;

        /// <summary>
        /// Gets or sets whether the tabs should be visible on this control or not
        /// </summary>
        [Browsable(true), Category("Appearance"), DefaultValue(false),
         Description("Hide or show the tabs in this panel")]
        public bool TabsVisible
        {
            get { return _tabsVisible; }
            set
            {
                _tabsVisible = value;
                Invalidate();
            }
        }

        [Browsable(true), Description("Disables the use of arrow keys to change tabs")]
        public bool DisableArrowKeys
        {
            get { return _disableArrowKeys; }
            set
            {
                _disableArrowKeys = value;
                Invalidate();
            }
        }

        /// <summary>
        /// Overrides the processing of windows messages to filter out the
        /// TCM_ADJUSTRECT message.
        /// </summary>
        /// <param name="m"></param>
        protected override void WndProc(ref Message m)
        {
            // Filter out TCM_ADJUSTRECT messages at runtime
            if (m.Msg != WindowsMessage.TCM_ADJUSTRECT || DesignMode || _tabsVisible)
                base.WndProc(ref m);
        }
        /// <summary>
        /// Overrides the OnKeyDown event to stop ctrl-alt-tab events being processed
        /// </summary>
        /// <param name="e"></param>
        protected override void OnKeyDown(KeyEventArgs e)
        {
            if (e.KeyData == (Keys.Tab | Keys.Control)
                | e.KeyData == (Keys.Tab | Keys.Control | Keys.Shift)
                | ((e.KeyData == Keys.Right || e.KeyData == Keys.Left) && _disableArrowKeys)
                )
                e.Handled = true;
            else
                base.OnKeyDown(e);
        }
    }

}
