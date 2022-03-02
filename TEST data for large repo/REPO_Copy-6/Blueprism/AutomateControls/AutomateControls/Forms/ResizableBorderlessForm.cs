using System;
using System.Drawing;
using System.Windows.Forms;

namespace AutomateControls.Forms
{

    public partial class ResizableBorderlessForm : Form
    {
        private const int WM_NCHITTEST = 0x84;
        private const int HTCLIENT = 1;
        private const int HTCAPTION = 2;
        private const int WS_BORDER = 0x40000;

        private Control _dragArea;
        private bool _dragging;
        private (Point cursor, Point form) _beforeDragLocation;
      
        public ResizableBorderlessForm()
        {
            InitializeComponent();
            WindowState = FormWindowState.Maximized;
            _dragArea = this;
            AddDragAreaEventHandlers();
        }
        
        protected Control DragArea
        {
            get => _dragArea;
            set
            {
                AddDragAreaEventHandlers();
                _dragArea = value;
            }
        }

        private void HandleDragAreaMouseDown(object sender, MouseEventArgs e)
        {
            _dragging = true;
            _beforeDragLocation = (Cursor.Position, Location);
        }

        private void HandleDragAreaMouseUp(object sender, MouseEventArgs e)
            => _dragging = false;

        private void HandleDragAreaMouseMove(object sender, MouseEventArgs e)
        {   
            if (!_dragging) return;
            var difference = Point.Subtract(Cursor.Position, new Size(_beforeDragLocation.cursor));
            Location = Point.Add(_beforeDragLocation.form, new Size(difference));
        }

        private void AddDragAreaEventHandlers()
        {
            _dragArea.MouseDown -= HandleDragAreaMouseDown;
            _dragArea.MouseUp -= HandleDragAreaMouseUp;
            _dragArea.MouseMove -= HandleDragAreaMouseMove;
            _dragArea.MouseDown += HandleDragAreaMouseDown;
            _dragArea.MouseUp += HandleDragAreaMouseUp;
            _dragArea.MouseMove += HandleDragAreaMouseMove;
        }

        protected override void WndProc(ref Message message)
        {
            base.WndProc(ref message);
            
            if (message.Msg == WM_NCHITTEST && message.Result == (IntPtr) HTCLIENT)
                message.Result = (IntPtr)HTCAPTION;

        }

        protected override CreateParams CreateParams
        {
            get
            {
                var createParams = base.CreateParams;
                createParams.Style |= WS_BORDER;
                return createParams;
            }
        }
    }
}
