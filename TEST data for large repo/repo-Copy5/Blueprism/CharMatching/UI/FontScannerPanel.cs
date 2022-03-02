using BluePrism.CharMatching.Properties;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using System.Drawing.Text;
using System.Drawing.Drawing2D;
using System.Linq;
using BluePrism.BPCoreLib;

namespace BluePrism.CharMatching.UI
{
    /// <summary>
    /// Panel which renders the scanning of system fonts looking for the font which
    /// rendered a pre-configured image.
    /// </summary>
    public partial class FontScannerPanel : UserControl
    {
        #region - Class Scope Declarations -

        /// <summary>
        /// A class to represent the arguments for the worker which is searching the
        /// system fonts for a match.
        /// </summary>
        private class WorkerArgs
        {
            // The text represented on the image.
            private string _text;

            // The image to compare against.
            private Bitmap _bmp;

            // The settings for the search
            private FontSearchDetailSettings _settings;

            /// <summary>
            /// Creates a new worker args object with the given attributes.
            /// </summary>
            /// <param name="txt">The text to render in an image</param>
            /// <param name="bmp">The reference image to compare against</param>
            /// <param name="settings">The settings governing the system fonts and
            /// styles to search</param>
            public WorkerArgs(string txt, Bitmap bmp, FontSearchDetailSettings settings)
            {
                _text = txt;
                _bmp = bmp;
                _settings = settings;
            }

            /// <summary>
            /// The text represented on the reference image
            /// </summary>
            public string Text { get { return _text; } }

            /// <summary>
            /// The reference image to check rendered images against
            /// </summary>
            public Bitmap Bitmap { get { return _bmp; } }

            /// <summary>
            /// The arguments detailing the system fonts to search
            /// </summary>
            public FontSearchDetailSettings Settings { get { return _settings; } }
        }

        /// <summary>
        /// Class to represent an interim progress update from the background worker
        /// which is scanning the fonts
        /// </summary>
        private class WorkerProgressUpdate
        {
            // The font checked
            private Font _font;
            // The message
            private string _message;

            /// <summary>
            /// Creates a new update object with the given message and no font.
            /// </summary>
            /// <param name="msg">The message to pass in the update</param>
            public WorkerProgressUpdate(string msg) : this(null, msg) { }

            /// <summary>
            /// Creates a new update object indicating a matched font with an
            /// accompanying message
            /// </summary>
            /// <param name="font">The font which has been found to be matched
            /// </param>
            /// <param name="msg">A message to report to the user.</param>
            public WorkerProgressUpdate(Font font, string msg)
            {
                _font = font;
                _message = msg;
            }
            /// <summary>
            /// A font which has been found to be matched
            /// </summary>
            public Font MatchingFont { get { return _font; } }

            /// <summary>
            /// A message to report to the user
            /// </summary>
            public string Message { get { return _message; } }

        }

        /// <summary>
        /// Basic class to represent the name and size of a font
        /// </summary>
        private class FontDescriptor
        {
            // The name of the font
            private string _name;

            // The size of the font
            private float _em;

            /// <summary>
            /// Creates a new font descriptor with the given name and size
            /// </summary>
            /// <param name="name">The name of the font</param>
            /// <param name="em">The size of the font, in 'em's</param>
            public FontDescriptor(string name, float em)
            {
                _name = name;
                _em = em;
            }

            /// <summary>
            /// The name of the font
            /// </summary>
            public string Name { get { return _name; } }

            /// <summary>
            /// The size of the font, in 'em's
            /// </summary>
            public float Em { get { return _em; } }
        }

        #endregion

        #region - Published Events -

        /// <summary>
        /// Event indicating that the work of scanning the system fonts has started.
        /// </summary>
        public event EventHandler WorkStarted;

        /// <summary>
        /// Event indicating that the work of scanning the system fonts has finished.
        /// </summary>
        public event EventHandler WorkFinished;

        #endregion

        #region - Member Variables -

        private SpyRegion _reg;

        private BackgroundWorker _worker;

        #endregion

        #region - Constructors -

        public FontScannerPanel()
        {
            InitializeComponent();
            _worker = new BackgroundWorker();
            _worker.WorkerReportsProgress = true;
            _worker.WorkerSupportsCancellation = true;
            _worker.DoWork += new DoWorkEventHandler(HandleWorkerDoWork);
            _worker.ProgressChanged += new ProgressChangedEventHandler(HandleWorkerProgressChanged);
            _worker.RunWorkerCompleted += new RunWorkerCompletedEventHandler(HandleWorkerCompleted);
            lblStatus.Text = "";
            lnkSearchDetails.Tag = new FontSearchDetailSettings(lnkSearchDetails.Text);
        }

        #endregion

        #region - Properties -

        /// <summary>
        /// The region from which the reference image is extracted
        /// </summary>
        public SpyRegion SpyRegion
        {
            get { return _reg; }
            set { _reg = value; OnRegionImageChanged(EventArgs.Empty); }
        }

        /// <summary>
        /// The rectangle that the spied region describes
        /// </summary>
        public Rectangle SpiedRegionRectangle
        {
            get { return (_reg == null ? default(Rectangle) : _reg.Rectangle); }
        }

        /// <summary>
        /// The reference image from within the spied region
        /// </summary>
        public Image SpiedRegionImage
        {
            get { return (_reg == null ? null : _reg.Image); }
        }

        /// <summary>
        /// The currently selected font in the list of fonts matched in the scan
        /// </summary>
        public Font SelectedFont
        {
            get
            {
                DataGridViewRow row = SelectedRow;
                return (row == null ? null : row.Tag as Font);
            }
        }

        /// <summary>
        /// The currently selected row in the list of fonts which were matched in
        /// the scan
        /// </summary>
        private DataGridViewRow SelectedRow => tableFonts.SelectedRows.Count == 0 ? null : tableFonts.SelectedRows.Cast<DataGridViewRow>().FirstOrDefault();


        /// <summary>
        /// Gets whether the font scanner is busy or not. It is busy if a background
        /// worker is currently scanning the system fonts and comparing them to the
        /// reference image.
        /// </summary>
        public bool Busy
        {
            get
            {
                return btnCancel.Enabled;
            }
            private set
            {
                btnCancel.Enabled = value;
                btnGo.Enabled = !value;
                txtExpected.Enabled = !value;
                if (value)
                {
                    btnCancel.Focus();
                }
                else
                {
                    txtExpected.Focus();
                }
            }
        }

        #endregion

        #region - Event Handlers -

        /// <summary>
        /// Handles the region image changing
        /// </summary>
        /// <param name="e"></param>
        private void OnRegionImageChanged(EventArgs e)
        {
            picbox.Image = SpiedRegionImage;
        }

        /// <summary>
        /// Handles the 'Go' button being pressed
        /// </summary>
        private void HandleGoPressed(object sender, EventArgs e)
        {
            string txt = txtExpected.Text.Trim();
            Bitmap src = ImageBender.AsBitmap(ImageBender.GrayscaleImage(picbox.Image, 0.8f));
            if (src != null && !src.Size.IsEmpty && txt.Length > 0)
            {
                this.Busy = true;
                tableFonts.Rows.Clear();
                OnWorkStarted(EventArgs.Empty);
                _worker.RunWorkerAsync(new WorkerArgs(
                    txt, src, lnkSearchDetails.Tag as FontSearchDetailSettings));
            }
        }

        /// <summary>
        /// Handles the background worker completing.
        /// </summary>
        void HandleWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            OnWorkFinished(EventArgs.Empty);
            this.Busy = false;
            if (e.Error != null)
            {
                MessageBox.Show(string.Format(Resources.ErrorOccurred0, e.Error), Resources.Error,
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            progress.Value = 100;
            lblStatus.Text = (e.Cancelled ? Resources.Cancelled : Resources.Complete);
        }

        /// <summary>
        /// Handles the background worker reporting some progress
        /// </summary>
        void HandleWorkerProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            progress.Value = e.ProgressPercentage;
            string msg = e.UserState as string;
            if (msg != null)
            {
                lblStatus.Text = msg;
            }
            else
            {
                Font f = e.UserState as Font;
                if (f != null)
                {
                    int i = tableFonts.Rows.Add(f.Name, f.Style.ToString(), f.Size);
                    // Save the font as the tag of the row
                    tableFonts.Rows[i].Tag = f;
                }
            }
        }

        /// <summary>
        /// Handles the work of the background worker.
        /// </summary>
        void HandleWorkerDoWork(object sender, DoWorkEventArgs e)
        {
            WorkerArgs args = (WorkerArgs)e.Argument;
            string txt = args.Text;
            Bitmap src = args.Bitmap;
            FontSearchDetailSettings settings = args.Settings;

            // Guess that the bg color is the first pixel and the font is in
            // the opposite color. Use the named colors for safety of checking later
            Color bgColor = ImageBender.GetPredominantColour(src);
            // normalise into the named colour.
            bgColor = (
                bgColor.ToArgb() == Color.Black.ToArgb() ? Color.Black : Color.White
            );

            // And treat FG as the opposite.
            Color fgColor = (bgColor == Color.Black ? Color.White : Color.Black);

            Size sz = src.Size;
            Rectangle imgRect = new Rectangle(Point.Empty, sz);
            List<Font> potentials = new List<Font>();
            List<FontDescriptor> faileds = new List<FontDescriptor>();
            using (Bitmap dest = new Bitmap(sz.Width, sz.Height, src.PixelFormat))
            {
                using (Graphics g = Graphics.FromImage(dest))
                {
                    g.CompositingQuality = CompositingQuality.HighSpeed;
                    g.InterpolationMode = InterpolationMode.NearestNeighbor;
                    g.TextRenderingHint = TextRenderingHint.SingleBitPerPixel;

                    StringFormat fmt = new StringFormat();
                    fmt.Trimming = StringTrimming.None;
                    fmt.FormatFlags = StringFormatFlags.NoWrap;
                    fmt.Alignment = StringAlignment.Near;

                    TextFormatFlags textFlags = TextFormatFlags.NoClipping |
                                        TextFormatFlags.NoPadding |
                                        TextFormatFlags.NoPrefix |
                                        TextFormatFlags.SingleLine;

                    using (Brush b = new SolidBrush(fgColor))
                    {
                        InstalledFontCollection fonts =
                            new InstalledFontCollection();
                        int total = fonts.Families.Length * settings.Ems.Count *
                            settings.Styles.Count * settings.RenderMethods.Count;
                        int curr = 0;

                        foreach (FontFamily fontFam in fonts.Families)
                        {
                            foreach (FontStyle style in settings.Styles)
                            {
                                // If this style is not available for this font,
                                // don't even bother trying it but update the counter
                                if (!fontFam.IsStyleAvailable(style))
                                {
                                    curr += settings.Ems.Count;
                                    break;
                                }

                                foreach (float em in settings.Ems)
                                {
                                    int progress = (100 * curr) / total;
                                    _worker.ReportProgress(progress,
                                        string.Format(Resources.Testing01,
                                        fontFam.Name, em));

                                    if (_worker.CancellationPending)
                                    {
                                        e.Cancel = true;
                                        return;
                                    }

                                    g.Clear(bgColor);
                                    Font f = null;
                                    try
                                    {
                                        f = new Font(fontFam, em, style);
                                    }
                                    catch
                                    {
                                        faileds.Add(new FontDescriptor(
                                            fontFam.Name, em));
                                    }

                                    foreach (RenderMethod method in
                                        settings.RenderMethods)
                                    {
                                        if (method == RenderMethod.GDI)
                                        {
                                            curr++;
                                            TextRenderer.DrawText(
                                                g, txt,
                                                f, imgRect, fgColor, textFlags);
                                        }
                                        else if (method == RenderMethod.GDIPlus)
                                        {
                                            curr++;
                                            g.DrawString(txt, f, b,
                                                (RectangleF)imgRect, fmt);
                                        }


                                        Rectangle box =
                                            ImageBender.FindRectangleOfColour(
                                            dest, fgColor);
                                        if (box.Height == 0 || box.Width == 0)
                                            continue;

                                        if (!box.IsEmpty)
                                        {
                                            Rectangle? rect = ImageBender.Contains(
                                                src, dest, box, fgColor);
                                            if (rect.HasValue)
                                            {
                                                _worker.ReportProgress(progress, f);
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Handles the cancel button being pressed
        /// </summary>
        private void HandleCancelClicked(object sender, EventArgs e)
        {
            if (this.Busy)
            {
                _worker.CancelAsync();
            }
        }

        /// <summary>
        /// Handles the selected font in the matched fonts table being changed
        /// </summary>
        private void HandleFontSelectionChanged(object sender, EventArgs e)
        {
            Image oldImage = picPreview.Image;
            if (tableFonts.SelectedRows.Count == 0)
            {
                picPreview.Image = null;
            }
            else
            {
                DataGridViewRow row = tableFonts.SelectedRows[0];
                Font f = row.Tag as Font;
                bool disposeOfFont = false;
                if (f == null)
                {
                    disposeOfFont = true;
                    string fontName = (string)row.Cells[0].Value;
                    FontStyle style = default(FontStyle);
                    clsEnum.TryParse((string)row.Cells[1].Value, ref style);
                    float em = (float)row.Cells[2].Value;
                    f = new Font(fontName, em, style);
                }
                Size sz = picbox.Image.Size;
                Bitmap bmp = new Bitmap(sz.Width, sz.Height);
                using (Graphics g = Graphics.FromImage(bmp))
                {
                    g.DrawString(txtExpected.Text, f, Brushes.Black, 0f, 0f);
                }
                picPreview.Image = bmp;
                if (disposeOfFont)
                    f.Dispose();
            }
            if (oldImage != null)
                oldImage.Dispose();
        }

        /// <summary>
        /// Handles the search detail settings link label being clicked
        /// </summary>
        private void HandleSearchDetailLinkClick(
            object sender, LinkLabelLinkClickedEventArgs e)
        {
            LinkLabel lbl = sender as LinkLabel;
            using (FontScannerSearchParamsForm f = new FontScannerSearchParamsForm())
            {
                f.Settings = lbl.Tag as FontSearchDetailSettings;
                if (f.ShowDialog() == DialogResult.OK)
                {
                    FontSearchDetailSettings s = f.Settings;
                    lbl.Tag = s;
                    lbl.Text = s.Encoded;
                }
            }
        }

        #endregion

        #region - Other Methods -

        /// <summary>
        /// Raises the <see cref="WorkStarted"/> event
        /// </summary>
        protected virtual void OnWorkStarted(EventArgs e)
        {
            if (WorkStarted != null)
                WorkStarted(this, e);
        }

        /// <summary>
        /// Raises the <see cref="WorkFinished"/> event
        /// </summary>
        protected virtual void OnWorkFinished(EventArgs e)
        {
            if (WorkFinished != null)
                WorkFinished(this, e);
        }

        #endregion

    }
}
