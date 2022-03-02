using AutomateControls.UIState.UIElements;
using System;
using System.Drawing;
using System.Windows.Forms;

namespace AutomateControls.SplitContainers
{
    public partial class HighlightingSplitContainer : GrippableSplitContainer
    {
        public Color FocusColor { get; set; } = ColourScheme.BluePrismControls.FocusColor;
        public Color ForeGroundColor { get; set; } = ColourScheme.BluePrismControls.ForeColor;
        public Color DisabledColor { get; set; } = ColourScheme.BluePrismControls.DisabledBackColor;
        public Color HoverColor { get; set; } = ColourScheme.BluePrismControls.HoverColor;
        public Color TextColor { get; set; } = ColourScheme.BluePrismControls.TextColor;
        public Color MouseLeaveColor { get; set; } = ColourScheme.BluePrismControls.MouseLeaveColor;

        public HighlightingSplitContainer()
        {
            splitterPreview = new SplitHighlight() { BackColor = FocusColor };
            InitializeComponent();

            SplitterMoving += OnSplitterMoving;
            SplitterMoved += OnSplitterMoved;
        }

        private SplitHighlight splitterPreview;
        private bool _isMouseHover = false;
        private bool _isSplitterMoving = false;

        private void OnSplitterMoving(object sender, SplitterCancelEventArgs e)
        {
            splitterPreview.Location = PointToScreen(new Point(e.SplitX, e.SplitY));

            var size = Orientation == Orientation.Vertical ? new Size(SplitterWidth, Height) : new Size(Width, SplitterWidth);
            splitterPreview.Size = size;
            if (!splitterPreview.Visible)
                splitterPreview.ShowInactiveTopmost();

            Cursor = Cursors.VSplit;

            _isSplitterMoving = true;
        }

        private void OnSplitterMoved(object sender, SplitterEventArgs e)
        {
            AutoSaveUserMove();
            _isSplitterMoving = false;
        }

        private void AutoSaveUserMove()
        {
            if (_isSplitterMoving)
                this.SaveUserLayout();
        }

        protected override void OnMouseUp(MouseEventArgs e)
        {
            base.OnMouseUp(e);
            splitterPreview.Hide();
            Cursor = Cursors.Default;
        }

        protected override void OnMouseHover(EventArgs e)
        {
            base.OnMouseHover(e);
            _isMouseHover = true;
            Refresh();
        }

        protected override void OnMouseLeave(EventArgs e)
        {
            base.OnMouseLeave(e);
            _isMouseHover = false;
            Refresh();
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            if (!_isSplitterMoving)
                this.LoadUserLayout();

            if (_isMouseHover)
                e.Graphics.FillRectangle(new SolidBrush(HoverColor), SplitterRectangle);
        }
    }
}
