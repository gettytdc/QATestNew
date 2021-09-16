using System;
using System.Windows.Forms;
using System.ComponentModel;

namespace AutomateControls
{
    /// <summary>
    /// Container for a full width flowlayoutpanel, which handles scrolling
    /// on behalf of that panel.
    /// Really, I'd rather this class didn't exist, but I couldn't get the
    /// scrolling to work properly in the FullWidthFlowLayoutPanel itself,
    /// so this is rather a pants workaround.
    /// 
    /// Note: This panel supports containing a single control only - that 
    /// being the full width flow layout panel that it creates itself.
    /// To add flowed controls, controls should be handled using the
    /// <see cref="FlowedControls"/> collection.
    /// </summary>
    public class FullWidthFlowLayoutScrollPane : System.Windows.Forms.Panel
    {
        // The panel which this panel is handling scrolling for.
        private FullWidthFlowLayoutPanel _flowPanel;

        /// <summary>
        /// Creates a new scrolling full width flow layout panel
        /// </summary>
        public FullWidthFlowLayoutScrollPane()
        {
            this.AutoScroll = true;
            this.DoubleBuffered = true;

            _flowPanel = new FullWidthFlowLayoutPanel();
            _flowPanel.Dock = DockStyle.Fill;
            // You need to set this flow panel to autoscroll, which I really
            // don't understand, but otherwise, this just doesn't work.
            _flowPanel.AutoScroll = true;
            Controls.Add(_flowPanel);
        }

        /// <summary>
        /// The delta magnitude to use for mouse wheel events - ie. the number of
        /// pixels by which this panel should scroll for a single mousewheel turn
        /// event. The amount of delta which generates an event is 120 - ie. scroll
        /// by 120 pixels, which for smaller panels is too much.
        /// </summary>
        [DisplayName("Mouse Wheel Delta Magnitude"), DefaultValue(120),
         Description("The number of pixels to scroll by for a single mousewheel turn")]
        public int MouseWheelDeltaMagnitude
        {
            get { return _flowPanel.MouseWheelDeltaMagnitude; }
            set { _flowPanel.MouseWheelDeltaMagnitude = value; }
        }

        /// <summary>
        /// The control collection containing the flowed controls which are
        /// being held in the flow layout panel managed by this scrollpane.
        /// </summary>
        public ControlCollection FlowedControls
        {
            get { return _flowPanel.Controls; }
        }

        /// <summary>
        /// Handles a control being added to this panel.
        /// Currently, this will fail for any control which is not the
        /// wrapped flow panel managed by this scroll pane.
        /// </summary>
        /// <param name="e"></param>
        protected override void OnControlAdded(System.Windows.Forms.ControlEventArgs e)
        {
            if (e.Control != _flowPanel)
            {
                throw new InvalidOperationException(
                    "Only a single full width flow panel is allowed on this control. " + 
                    "Use the FlowedControls property");
            }
            base.OnControlAdded(e);
        }

        /// <summary>
        /// The underlying flow panel being managed by this scroll pane.
        /// </summary>
        public FullWidthFlowLayoutPanel FlowPanel
        {
            get { return _flowPanel; }
        }

        /// <summary>
        /// Disposes of this scroll pane, ensuring that the flow panel being
        /// managed by it is disposed too.
        /// </summary>
        /// <param name="disposing"></param>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
                _flowPanel.Dispose();
            base.Dispose(disposing);
        }
    }
}
