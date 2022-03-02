using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using System.Drawing.Text;
using BluePrism.BPCoreLib;

namespace BluePrism.CharMatching.UI
{
    /// <summary>
    /// Panel with which a font can be identified by comparing it manually against
    /// renderings from installed system fonts.
    /// </summary>
    public partial class FontBrowserPanel : UserControl
    {
        // format used to render the string
        private StringFormat _fmt;

        // The region containing the rendering image
        private SpyRegion _reg;

        /// <summary>
        /// Creates a new empty font browser panel
        /// </summary>
        public FontBrowserPanel()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FontBrowserPanel));
            InitializeComponent();
            cmbSize.SelectedItem = "10";
            cmbStyle.SelectedItem = resources.GetString("cmbStyle.Items");
            lbFontRenders.DrawMode = DrawMode.OwnerDrawVariable;
            lbFontRenders.DrawItem += new DrawItemEventHandler(HandleListDrawItem);
            lbFontRenders.MeasureItem += new MeasureItemEventHandler(HandleListMeasureItem);
            _fmt = new StringFormat();
            _fmt.FormatFlags = StringFormatFlags.NoClip;
        }

        /// <summary>
        /// Gets or sets the region containing the reference image - ie. the image
        /// which will be compared against the generated images to ascertain the
        /// font.
        /// </summary>
        public SpyRegion SpyRegion
        {
            get { return _reg; }
            set
            {
                if (_reg != value)
                {
                    _reg = value;
                    picbox.Image = (_reg == null ? null : _reg.Image);
                    btnRender.Enabled = txtEntered.Text.Length > 0 && _reg != null;
                }
            }
        }

        /// <summary>
        /// Handles the measuring of the space required for the list item.
        /// </summary>
        void HandleListMeasureItem(object sender, MeasureItemEventArgs e)
        {
            if (e.Index < 0 || e.Index >= lbFontRenders.Items.Count)
                return;
            Font f = lbFontRenders.Items[e.Index] as Font;
            if (f == null)
                return;
            SizeF sz = e.Graphics.MeasureString(
                txtEntered.Text, f, lbFontRenders.Width, _fmt);
            e.ItemHeight = (int)sz.Height;
            e.ItemWidth = (int)sz.Width;
        }

        /// <summary>
        /// Handles the drawing of the list item.
        /// </summary>
        void HandleListDrawItem(object sender, DrawItemEventArgs e)
        {
            if (e.Index < 0 || e.Index >= lbFontRenders.Items.Count)
                return;
            Font f = lbFontRenders.Items[e.Index] as Font;
            if (f == null)
                return;
            bool selected =
                ((e.State & (DrawItemState.Selected | DrawItemState.HotLight)) != 0);

            // Background
            e.Graphics.FillRectangle(
                selected ? SystemBrushes.Highlight : SystemBrushes.Control, e.Bounds);

            // Foreground
            e.Graphics.DrawString(f.Name, f, 
                selected ? SystemBrushes.HighlightText : SystemBrushes.WindowText,
                e.Bounds.Location, _fmt);
        }

        /// <summary>
        /// Handles a notification that a re-render is required.
        /// </summary>
        private void HandleReRenderRequired(object sender, EventArgs e)
        {
            RenderGeneratedImage();
        }

        /// <summary>
        /// Renders the generated image from the currently set values in the panel
        /// </summary>
        private void RenderGeneratedImage()
        {
            Font f = lbFontRenders.SelectedItem as Font;
            int size = SelectedFontSize;
            FontStyle style = SelectedFontStyle;
            string txt = txtEntered.Text;

            if (f == null || size == 0 || txt == "")
            {
                txtFontName.Text = "";
                Image img = picboxPreview.Image;
                if (img != null)
                    img.Dispose();
            }
            else
            {
                txtFontName.Text = f.Name;

                Image img = picboxPreview.Image;
                if (img != null)
                    img.Dispose();

                Bitmap bmp = new Bitmap(picboxPreview.Width, picboxPreview.Height);
                using (Graphics g = Graphics.FromImage(bmp))
                {
                    using (Font renderFont = new Font(f.Name, size, style))
                    {
                        g.DrawString(txt, renderFont, Brushes.Black, PointF.Empty);
                    }
                    g.Flush();
                }
                picboxPreview.Image = bmp;
                picboxPreview.Invalidate();
            }
        }

        /// <summary>
        /// The selected size of the font
        /// </summary>
        [Browsable(false),
         DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public int SelectedFontSize
        {
            get
            {
                int size;
                if (!int.TryParse(cmbSize.SelectedItem as String, out size))
                    return 0;
                return size;
            }
        }

        /// <summary>
        /// The selected style of the font
        /// </summary>
        [Browsable(false),
         DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public FontStyle SelectedFontStyle
        {
            get
            {
                FontStyle style = default(FontStyle);
                clsEnum.TryParse(cmbStyle.SelectedItem as String, ref style);
                return style;
            }
        }

        /// <summary>
        /// The selected font.
        /// </summary>
        [Browsable(false),
         DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public Font SelectedFont
        {
            get
            {
                string name = txtFontName.Text;
                int size = SelectedFontSize;
                FontStyle style = SelectedFontStyle;
                if (name == "" || size == 0)
                    return null;
                return new Font(name, size, style);
            }
        }

        /// <summary>
        /// Handles the rendering of the list
        /// </summary>
        private void HandleRenderList(object sender, EventArgs e)
        {
            Font selFont = lbFontRenders.SelectedItem as Font;
            string selName = (selFont == null ? null : selFont.Name);
            lbFontRenders.BeginUpdate();
            try
            {
                foreach (Font f in lbFontRenders.Items)
                {
                    f.Dispose();
                }
                lbFontRenders.Items.Clear();

                int size = SelectedFontSize;
                FontStyle style = SelectedFontStyle;

                InstalledFontCollection fonts = new InstalledFontCollection();
                foreach (FontFamily fam in fonts.Families)
                {
                    if (fam.IsStyleAvailable(style))
                    {
                        Font f = new Font(fam.Name, size, style);
                        lbFontRenders.Items.Add(f);
                        if (fam.Name == selName)
                            lbFontRenders.SelectedItem = f;
                    }
                }
            }
            finally
            {
                lbFontRenders.EndUpdate();
            }
        }

        /// <summary>
        /// Handles the text being changed, ensuring that the render button is
        /// enabled if text has been entered.
        /// </summary>
        private void HandleTextChanged(object sender, EventArgs e)
        {
            btnRender.Enabled = (txtEntered.Text.Length > 0 && _reg != null);
        }

    }
}
