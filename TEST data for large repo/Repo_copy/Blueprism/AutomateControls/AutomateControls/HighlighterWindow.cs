using System;
using System.Windows.Forms;
using System.Drawing;
using System.ComponentModel;
using System.Threading.Tasks;

namespace AutomateControls
{
    /// <summary>
    /// Window with which an area of the screen can be highlighted
    /// </summary>
    public class HighlighterWindow : IDisposable
    {
        #region - Class Scope Declarations -

        /// <summary>
        /// The padding used for the higlighting
        /// </summary>
        private static readonly Size HighlightPadding = new Size(2, 2);

        /// <summary>
        /// Shows a highlight rectangle at the specified screen co-ordinates for
        /// the given amount of time. The calling thread is blocked until the
        /// specified time has run out.
        /// </summary>
        /// <param name="parent">The parent control which should own the resulting
        /// HighlighterWindow</param>
        /// <param name="screenCoords">The co-ordinates to display a highlight
        /// rectangle</param>
        /// <param name="time">The amount of time to display the rectangle for.
        /// </param>
        public static void ShowFor(Control parent, Rectangle screenCoords, TimeSpan time)
        {
            using (var hw = new HighlighterWindow())
            {
                hw.HighlightScreenRect = screenCoords;
                hw.Visible = true;
                hw.ShowDialog(parent, time);
            }
        }

        /// <summary>
        /// Shows a highlight rectangle at the specified screen co-ordinates for
        /// the given amount of time. The method returns immediately after the
        /// window has been shown.
        /// </summary>
        /// <param name="parent">The parent control which should own the resulting
        /// HighlighterWindow</param>
        /// <param name="screenCoords">The co-ordinates to display a highlight
        /// rectangle</param>
        /// <param name="time">The amount of time to display the rectangle for.
        /// </param>
        public static void ShowForAsync(Control parent, Rectangle screenCoords, TimeSpan time)
        {
            HighlighterWindow hw = new HighlighterWindow();
            hw.HighlightScreenRect = screenCoords;
            hw.Visible = true;
            hw.Show(parent, time);
        }

        #endregion

        #region - Member Variables -

        // The colour to use to highlight the configured rectangle
        private Color _highlightColor;

        /// Whether the highlighter is visible.
        private bool _visible;

        /// The left highlight window.
        private Form _left;

        /// The right highlight window.
        private Form _right;

        /// The top highlight window.
        private Form _top;

        /// The bottom highlight window.
        private Form _bottom;

        /// Is disposed flag
        private bool _disposed = false;
        #endregion

        #region - Constructors -

        /// <summary>
        /// Creates a new highlighter window, set to highlight in red
        /// </summary>
        public HighlighterWindow() : this(Color.Red) { }

        /// <summary>
        /// Creates a new highlighter window set to highlight in the given colour
        /// </summary>
        /// <param name="highlightCol">The colour of the required highlight rectangle
        /// </param>
        public HighlighterWindow(Color highlightCol)
        {
            _left = CreateWindow();
            _right = CreateWindow();
            _top = CreateWindow();
            _bottom = CreateWindow();
            HighlightColor = highlightCol;
        }

        /// <summary>
        /// Impl of the IDispose pattern
        /// </summary>
        ~HighlighterWindow()
        {
            Dispose(false);
        }

        /// <summary>
        /// Dispose of the object
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private Form CreateWindow() => new Form
        {
            TopMost = true,
            ShowInTaskbar = false,
            FormBorderStyle = FormBorderStyle.None,
            StartPosition = FormStartPosition.Manual,
            // Show the form way off the screen - we want to create a handle
            // immediately so that it doesn't resize the window on creation
            // after the highlight rectangle has been set.
            Location = new Point(-10000, -10000),
            Size = new Size(20, 20),
            Visible = true,
            BackColor = HighlightColor
        };

        #endregion

        #region - Properties -

        /// <summary>
        /// The colour of the highlight rectangle to display
        /// </summary>
        [Browsable(true), Category("Appearance"), DefaultValue(typeof(Color), "Red"),
         Description("Sets the colour of the highlighting rectangle")]
        public Color HighlightColor
        {
            get => _highlightColor;
            set
            {
                _highlightColor = value;
                _left.BackColor = _highlightColor;
                _right.BackColor = _highlightColor;
                _top.BackColor = _highlightColor;
                _bottom.BackColor = _highlightColor;
            }
        }

        /// <summary>
        /// The bounds of the rectangle to display, in screen co-ordinates
        /// </summary>
        [Browsable(true), Category("Appearance"),
         Description("The screen co-ordinates to highlight")]
        public Rectangle HighlightScreenRect
        {
            get => m_Bounds;
            set
            {
                m_Bounds = value;
                UpdatePosition(m_Bounds);
            }
        }

        private Rectangle m_Bounds;

        private void UpdatePosition(Rectangle b)
        {
            var p = HighlightPadding;
            _left.Bounds = new Rectangle(b.Left, b.Top, p.Width, b.Height);
            _right.Bounds = new Rectangle(b.Right - p.Width, b.Top, p.Width, b.Height);
            _top.Bounds = new Rectangle(b.Left, b.Top, b.Width, p.Height);
            _bottom.Bounds = new Rectangle(b.Left, b.Bottom - p.Height, b.Width, p.Height);
        }

        #endregion

        #region - Methods -

        /// <summary>
        /// Shows this window for the given amount of time and then returns.
        /// Note that the calling thread is blocked until the time has expired and
        /// the window is hidden.
        /// </summary>
        /// <param name="parent">The owner of this window for the purposes of showing
        /// it.</param>
        /// <param name="forTime">The amount of time that the window should be shown
        /// for.</param>
        /// <remarks>This method <em>does not</em> close and dispose of the window
        /// after the allotted time - it merely hides it.</remarks>
        public void ShowDialog(Control parent, TimeSpan forTime)
        {
            // We can't 'show dialog' if it's already visible, so we hide it if it's currently showing
            if (Visible)
                Visible = false;

            if (forTime > TimeSpan.Zero)
                SetWindowDisplayDelayAndDispose(forTime);

            ShowDialog(parent);
        }

        /// <summary>
        /// Shows the highlighter windows and blocks.
        /// </summary>
        /// <param name="parent">The parent control of the window.</param>
        public void ShowDialog(Control parent)
        {
            _right.Show(parent);
            _top.Show(parent);
            _bottom.Show(parent);

            //Ensure at least one blocks.
            _left.ShowDialog(parent);
        }

        /// <summary>
        /// Shows this window for the given amount of time and then hides it.
        /// Note that the method returns immediately, leaving the window open for the
        /// specified amount of time.
        /// </summary>
        /// <param name="parent">The owner of this window for the purposes of showing
        /// it.</param>
        /// <param name="forTime">The amount of time that the window should be shown
        /// for.</param>
        /// <remarks>This method <em>does not</em> close and dispose of the window
        /// after the allotted time - it merely hides it.</remarks>
        public void Show(Control parent, TimeSpan forTime)
        {
            if (forTime > TimeSpan.Zero)
                SetWindowDisplayDelayAndDispose(forTime);
          
            if (!Visible)
                Show(parent);
        }


        /// <summary>
        /// Ensures the highlighter window is displayed for specific amount of time, and is hidden and disposed on the UI thread context.
        /// </summary>
        private void SetWindowDisplayDelayAndDispose(TimeSpan forTime)
        {
            var context = TaskScheduler.FromCurrentSynchronizationContext();
            Task.Delay(forTime).ContinueWith(HideAndDispose, context);
        }

        private void HideAndDispose(Task t)
        {
            try
            {
                Hide();
                Dispose();
            }
            catch
            {
                // do nothing
            }
        }

        /// <summary>
        /// Sets the highlighter visible.
        /// </summary>
        public bool Visible
        {
            get => _visible;
            set
            {
                _visible = value;
                _left.Visible = _visible;
                _right.Visible = _visible;
                _top.Visible = _visible;
                _bottom.Visible = _visible;
            }
        }

        /// <summary>
        /// Shows the highlighter
        /// </summary>
        /// <param name="parent"></param>
        public void Show(Control parent)
        {
            _left.Show(parent);
            _right.Show(parent);
            _top.Show(parent);
            _bottom.Show(parent);
            _visible = true;
        }

        /// <summary>
        /// Hides the highlighter
        /// </summary>
        public void Hide() => Visible = false;

        /// <summary>
        /// Determines whether the calling thread should
        /// use Invoke.
        /// </summary>
        public bool InvokeRequired => _left.InvokeRequired;

        /// <summary>
        /// Invokes the method via the ui thread.
        /// </summary>
        /// <param name="method">The method to invoke</param>
        /// <param name="args">The arguments to the method</param>
        public void Invoke(Delegate method, params object[] args)
        {
            _left.Invoke(method, args);
        }

        /// <summary>
        /// Disposes the Highlighter
        /// </summary>
        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    _left.Dispose();
                    _right.Dispose();
                    _top.Dispose();
                    _bottom.Dispose();
                }
                _disposed = true;
            }
        }

        #endregion
    }
}
