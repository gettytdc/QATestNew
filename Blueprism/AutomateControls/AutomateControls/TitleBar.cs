using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace AutomateControls
{
    public partial class TitleBar : System.Windows.Forms.UserControl
    {
        #region - Class-scope Declarations -

        /// <summary>
        /// The different logos available at the far right of the title bar
        /// </summary>
        public enum LogoImage
        {
            NoLogo,
            AutomateLogo,
            ProcessStudioLogo,
            ObjectStudioLogo,
        }

        private static readonly Color DefaultTitleColor = Color.Black;
        private static readonly Color DefaultSubtitleColor = Color.Black;

        private static readonly Color DefaultGradientFrom = Color.FromArgb(255, 198, 198, 208);
        private static readonly Color DefaultGradientTo = Color.FromArgb(255, 82, 99, 193);
        private static readonly Color DefaultStriping = Color.FromArgb(255, 159, 164, 193);

        private static readonly Point DefaultTitlePosition = new Point(10, 10);
        private static readonly Point DefaultSubtitlePosition = new Point(20, 30);
    
        #endregion

        #region - Constructors -

        public TitleBar()
        {
            //This call is required by the Windows Form Designer.
            InitializeComponent();

            //Add any initialization after the InitializeComponent() call
            SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.ResizeRedraw |
                ControlStyles.UserPaint | ControlStyles.OptimizedDoubleBuffer, true);

            BackColor = ColourScheme.Default.TitleBarColor;

        }

        #endregion

        #region - Private Member Variables -

        private string _title;
        private string _subtitle;
        private Point _titlePosn = new Point(10, 10);
        private Point _subtitlePosn = new Point(20, 30);
        private Color _titleColor = ColourScheme.Default.TitleBarText;
        private Color _subtitleColor = ColourScheme.Default.TitleBarText;
        private Font _titleFont;
        private Font _subtitleFont;
        private Brush _titleBrush;
        private Brush _subtitleBrush;
        private bool _wrapTitle;

        #endregion

        #region - Designer Helper Methods -

        private bool ShouldSerializeTitleFont()
        {
            return (_titleFont != null);
        }

        private void ResetTitleFont()
        {
            _titleFont = null;
        }

        private bool ShouldSerializeSubtitleFont()
        {
            return (_subtitleFont != null);
        }

        private void ResetSubtitleFont()
        {
            _subtitleFont = null;
        }

        #endregion

        #region - Designer-visible Properties -

        /// <summary>
        /// The background colour of this titlebar.
        /// </summary>
        [DefaultValue(typeof(Color),ColourScheme.Default.EnvironmentBackColorRGB)]
        public override Color BackColor
        {
            get { return base.BackColor; }
            set { base.BackColor = value; }
        }
        /// <summary>
        /// Overridden forecolor just to hide it from the editor / designer. It has
        /// no effect on the title / subtitle text so it can be misleading
        /// </summary>
        [Browsable(false), EditorBrowsable(EditorBrowsableState.Never),
         DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public override Color ForeColor
        {
            get { return base.ForeColor; }
            set { base.ForeColor = value; }
        }

        /// <summary>
        /// The text to use in the title
        /// </summary>
        [Browsable(true), Category("Appearance"), DefaultValue(""),
         Description("The text to use for the title"), Localizable(true)]
        public string Title
        {
            get { return (_title ?? ""); }
            set { _title = value; Invalidate(); }
        }

        [Browsable(true), Category("Appearance"), DefaultValue(""),
         Description("Whether or not the title text should wrap to the next line"), Localizable(true)]
        public bool WrapTitle
        {
            get { return _wrapTitle; }
            set { _wrapTitle = value; Invalidate(); }
        }

        /// <summary>
        /// The text to use in the subtitle
        /// </summary>
        [Browsable(true), Category("Appearance"), DefaultValue(""),
         Description("The text to use for the subtitle"), Localizable(true)]
        public string SubTitle
        {
            get { return (_subtitle ?? ""); }
            set { _subtitle = value; Invalidate(); }
        }

        /// <summary>
        /// The font to use for the title text.
        /// By default, this uses the parent's font settings
        /// </summary>
        [Browsable(true), Category("Appearance"),
         AmbientValue(null), Description("The font to use for the title")]
        public Font TitleFont
        {
            get { return (_titleFont == null ? this.Font : _titleFont); }
            set { _titleFont = value; Invalidate(); }
        }

        /// <summary>
        /// The font used to write the subtitle
        /// </summary>
        [Browsable(true), Category("Appearance"),
         AmbientValue(null), Description("The font to use for the title")]
        public Font SubtitleFont
        {
            get { return (_subtitleFont == null ? this.Font : _subtitleFont); }
            set { _subtitleFont = value; Invalidate(); }
        }

        /// <summary>
        /// The colour of the title text
        /// </summary>
        [Browsable(true), Category("Appearance"),
         DefaultValue(typeof(Color), "White"),
         Description("The colour to use to display the title")]
        public Color TitleColor
        {
            get { return _titleColor; }
            set
            {
                _titleColor = value;
                if (_titleBrush != null)
                {
                    _titleBrush.Dispose();
                    _titleBrush = null;
                }
                Invalidate();
            }
        }

        /// <summary>
        /// The colour of the subtitle text.
        /// </summary>
        [Browsable(true), Category("Appearance"),
         DefaultValue(typeof(Color), "White"),
         Description("The colour to use to display the subtitle")]
        public Color SubtitleColor
        {
            get { return _subtitleColor; }
            set
            {
                _subtitleColor = value;
                if (_subtitleBrush != null)
                {
                    _subtitleBrush.Dispose();
                    _subtitleBrush = null;
                }
                Invalidate();
            }
        }

        /// <summary>
        /// The position at which the title text is drawn
        /// </summary>
        [Browsable(true), Category("Appearance"),
         DefaultValue(typeof(Point), "10, 10"),
         Description("The relative position of the title text")]
        public Point TitlePosition
        {
            get { return _titlePosn; }
            set { _titlePosn = value; Invalidate(); }
        }

        /// <summary>
        /// The position at which the subtitle text is drawn
        /// </summary>
        [Browsable(true), Category("Appearance"),
         DefaultValue(typeof(Point), "20, 30"),
         Description("The relative position of the subtitle text")]
        public Point SubtitlePosition
        {
            get { return _subtitlePosn; }
            set { _subtitlePosn = value; Invalidate(); }
        }

        #endregion

        #region - Other Properties -

        /// <summary>
        /// The brush to use to write the subtitle text
        /// </summary>
        private Brush SubtitleBrush
        {
            get
            {
                if (_subtitleBrush == null)
                    _subtitleBrush = new SolidBrush(SubtitleColor);
                return _subtitleBrush;
            }
        }

        /// <summary>
        /// The brush to use to write the title text.
        /// </summary>
        private Brush TitleBrush
        {
            get
            {
                if (_titleBrush == null)
                    _titleBrush = new SolidBrush(TitleColor);
                return _titleBrush;
            }
        }

        #endregion

        /// <summary>
        /// Paints this control.
        /// </summary>
        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            Graphics g = e.Graphics;

            g.Clear(BackColor);

            //draw title and subtitle
            if (this.WrapTitle)
            {
                g.DrawString(_title,
                    TitleFont, TitleBrush, new Rectangle(TitlePosition.X, TitlePosition.Y, this.Width, this.Height));
            }
            else
            {
                g.DrawString(_title,
                    TitleFont, TitleBrush, TitlePosition.X, TitlePosition.Y);
            }

            g.DrawString(_subtitle,
                SubtitleFont, SubtitleBrush, SubtitlePosition.X, SubtitlePosition.Y);
        }

        /// <summary>
        /// If the given reference is not null, this disposes of it and
        /// sets it to null.
        /// </summary>
        /// <typeparam name="T">The type of IDisposable referred to</typeparam>
        /// <param name="disp">The disposable object to be disposed of and
        /// have its reference set to null</param>
        /// <remarks>The setting to null is only so that if it is called twice for
        /// the same object, this method will not attempt to dispose of it again.
        /// </remarks>
        private void DisposeAndNullify<T>(ref T disp) where T : class, IDisposable
        {
            if (disp != null)
            {
                disp.Dispose();
                disp = null;
            }
        }

        /// <summary>
        /// Disposes of this title bar
        /// </summary>
        /// <param name="explicitly">true if being called explicitly, false if
        /// being called implicitly in finalization</param>
        protected override void Dispose(bool explicitly)
        {
            if (explicitly)
            {
                if (components != null)
                {
                    components.Dispose();
                }
                DisposeAndNullify(ref _titleBrush);
                DisposeAndNullify(ref _subtitleBrush);
                DisposeAndNullify(ref _titleFont);
                DisposeAndNullify(ref _subtitleFont);
                //DisposeAndNullify(ref _icon);
                //DisposeAndNullify(ref _logoImg);
                //DisposeAndNullify(ref _rightIcon);
            }
            base.Dispose(explicitly);
        }
    }
}
