using BluePrism.CharMatching.Properties;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using System.IO;
using BluePrism.BPCoreLib.Collections;
using BluePrism.BPCoreLib;
using AutomateControls.Forms;
using BluePrism.Images;

namespace BluePrism.CharMatching.UI
{
    /// <summary>
    /// Control for managing the fonts in the current environment
    /// </summary>
    public partial class FontManager : UserControl
    {
        #region - Members -

        /// <summary>
        /// Event fired when a spy operation has been requested within this control
        /// </summary>
        public event SpyRequestEventHandler SpyRequested;

        public event EventHandler ReferencesRequested;

        // The store used to load / save / delete fonts
        private IFontStore _store;

        #endregion

        #region - Constructors -

        /// <summary>
        /// Creates a new font manager
        /// </summary>
        public FontManager()
        {
            InitializeComponent();

            this.btnNew.Image = ToolImages.New_16x16;
            this.btnExport.Image = ToolImages.Export_16x16;
            this.btnEdit.Image = ToolImages.Document_Edit_16x16;
            this.btnDelete.Image = ToolImages.Delete_Red_16x16;
            this.btnReferences.Image = ToolImages.Site_Map2_16x16;

            stripFontMenu.Enabled = false;
        }

        #endregion

        #region - Properties -

        /// <summary>
        /// The store with which fonts can be loaded, saved and deleted
        /// </summary>
        public IFontStore Store
        {
            get { return _store; }
            set
            {
                _store = value;
                LoadFromStore();
                stripFontMenu.Enabled = (_store != null);
            }
        }

        /// <summary>
        /// Gets the names of the currently selected fonts in this manager
        /// </summary>
        public ICollection<string> SelectedFontNames
        {
            get
            {
                List<string> fonts = new List<string>();
                foreach (DataGridViewRow row in gridFonts.SelectedRows)
                    fonts.Add(row.Cells[0].Value as string);
                return fonts;
            }
        }

        #endregion

        #region - Methods -

        /// <summary>
        /// Loads the fonts from the installed store and sets them in the grid
        /// </summary>
        public void LoadFromStore()
        {
            if (_store == null)
                return;

            gridFonts.Rows.Clear();
            foreach (string name in _store.AvailableFontNames)
            {
                string ocr = "";
                string ocrName = name;
                ocrName = ocrName.Replace("(", "");
                ocrName = ocrName.Replace(")", "");
                try
                {
                    ocr = "*";
                    _store.GetFontOcrPlus(ocrName);
                }
                catch
                {
                    ocr = "";
                }

                gridFonts.Rows.Add(name, ocr);
            }
        }

        /// <summary>
        /// Handles the font selection changing in this font manager
        /// </summary>
        private void HandleFontSelectionChanged(object sender, EventArgs e)
        {
            int selectCount = gridFonts.SelectedRows.Count;
            bool selected = (selectCount > 0);
            bool multiSelected = (selectCount > 1);

            btnDelete.Enabled = selected;
            btnEdit.Enabled = selected && !multiSelected;
            btnExport.Enabled = selected && !multiSelected;
        }

        /// <summary>
        /// Handles a new font being requested from a system font.
        /// </summary>
        private void HandleNewFromSystemFont(object sender, EventArgs e)
        {
            using (FontGeneratorForm f = new FontGeneratorForm())
            {
                f.Store = _store;
                f.ShowDialog();
            }
            LoadFromStore();
        }

        /// <summary>
        /// Handles a new font being requested from a region
        /// </summary>
        private void HandleNewFromRegions(object sender, EventArgs e)
        {
            SpyRequestEventArgs sre = new SpyRequestEventArgs();
            OnSpyRequested(sre);
            Image img = sre.SpiedImage;
            if (img != null)
            {
                using (FontEditorForm frm = new FontEditorForm())
                {
                    frm.Store = _store;
                    frm.SpyRequested += HandleSpyRequested;
                    ICollection<string> fontNames = _store.AvailableFontNames;
                    string fmtString = Resources.NewFont0;

                    string name;
                    int i = 0;
                    while (FontExists(name = string.Format(fmtString, ++i), fontNames))
                        ;
                    BPFont f = new BPFont(name, "1.0", new FontData());
                    frm.FontValue = f;
                    frm.Image = img;
                    if (frm.ShowDialog() == DialogResult.OK)
                    {
                        name = (f.Name ?? "").Trim();
                        if (name == "")
                        {
                            name = TextEnterBox.Show(Resources.Error,
                                Resources.YouMustProvideAFontName, "", false);
                            if (name == null) // cancelled
                                return;
                        }
                        while (FontExists(name, fontNames))
                        {
                            name = TextEnterBox.Show(Resources.Error,
                                string.Format(Resources.TheName0IsAlreadyInUsePleaseChooseAnother, name), name, false);
                            if (name == null) // cancelled
                                return;
                        }
                        f.Name = name;
                        _store.SaveFont(f);
                        LoadFromStore();
                    }
                }
            }
        }

        /// <summary>
        /// Check if font already exists
        /// </summary>
        private Boolean FontExists(string name, ICollection<string> fontNames)
        {
            foreach (string font in fontNames)
            {
                if (name.Equals(font, StringComparison.CurrentCultureIgnoreCase))
                    return true;
            }
            return false;
        }

        /// <summary>
        /// Raises the <see cref="SpyRequested"/> event
        /// </summary>
        /// <param name="e"></param>
        protected virtual void OnSpyRequested(SpyRequestEventArgs e)
        {
            SpyRequestEventHandler handler = this.SpyRequested;
            if (handler != null)
                handler(this, e);
        }

        /// <summary>
        /// Handles creating a new font by importing it from a file.
        /// </summary>
        private void HandleNewFromImport(object sender, EventArgs e)
        {
            using (OpenFileDialog fo = new OpenFileDialog())
            {
                fo.AddExtension = true;
                fo.Filter = Resources.BluePrismFontsXmlBpfontXmlBpfont;
                fo.Multiselect = true;
                if (fo.ShowDialog() == DialogResult.OK)
                {
                    ICollection<string> fontNames = _store.AvailableFontNames;
                    foreach (string filename in fo.FileNames)
                    {
                        try
                        {
                            BPFont f = new BPFont(new FileInfo(filename));
                            if (FontExists(f.Name, fontNames))
                            {
                                MessageBox.Show(null, string.Format(
                                    Resources.UnableToImportFile0AFontNamed1AlreadyExists,
                                    filename, f.Name), Resources.Error, MessageBoxButtons.OK);
                            }
                            else
                                _store.SaveFont(f);
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show(null, string.Format(
                                Resources.AnErrorOccurredWhileImporting01,
                                filename, ex.Message), Resources.Error,
                                MessageBoxButtons.OK, MessageBoxIcon.Error);

                            // FIXME: This would be another nice one to have in
                            // AutomateControls
                            //UserMessage.Show(string.Format(
                            //    "An error occurred while importing {0} : {1}",
                            //    filename, ex.Message), ex);
                        }
                    }
                }
            }
            LoadFromStore();
        }

        /// <summary>
        /// Handles exporting of the currently selected font(s)
        /// </summary>
        private void HandleExportClick(object sender, EventArgs e)
        {
            ICollection<string> fonts = SelectedFontNames;
            if (fonts.Count == 0)
                return;

            if (fonts.Count == 1)
            {
                using (SaveFileDialog sfd = new SaveFileDialog())
                {
                    sfd.AddExtension = true;
                    sfd.DefaultExt = "bpfont";
                    sfd.Filter = Resources.BluePrismFontsBpfontXmlBpfontXml;

                    BPFont f = _store.GetFont(CollectionUtil.First(fonts));
                    sfd.FileName = BPUtil.CleanFileName(f.Name);

                    if (sfd.ShowDialog() == DialogResult.OK)
                    {
                        f.ExportData(sfd.FileName);
                    }
                }
            }
            else
            {
                FolderBrowserDialog fo = new FolderBrowserDialog();
                if (fo.ShowDialog() == DialogResult.OK)
                {
                    foreach (string font in fonts)
                    {
                        BPFont f = _store.GetFont(font);
                        f.ExportData(Path.Combine(fo.SelectedPath,
                            BPUtil.CleanFileName(font) + ".bpfont"));
                    }
                }
            }
        }

        /// <summary>
        /// Finds the datagridview row which corresponds to a font with the given
        /// name.
        /// </summary>
        /// <param name="name">The font name for which the datagridview row is
        /// required.</param>
        /// <returns>The row corresponding to the specified name, or null if the
        /// given name was not found in the gridview.</returns>
        private DataGridViewRow FindFontRow(string name)
        {
            foreach (DataGridViewRow row in gridFonts.Rows)
            {
                if (object.Equals(row.Cells[0].Value, name))
                {
                    return row;
                }
            }
            return null;
        }

        /// <summary>
        /// Handles the Edit button being clicked.
        /// </summary>
        private void HandleEditClick(object sender, EventArgs e)
        {
            DataGridViewRow row = gridFonts.CurrentRow;
            if (row == null)
                return;
            try
            {
                using (FontEditorForm fe = new FontEditorForm())
                {
                    fe.Store = this.Store;
                    fe.SpyRequested += HandleSpyRequested;
                    fe.RegionEditorVisible = false;
                    
                    BPFont orig = Store.GetFont(row.Cells[0].Value as String);
                    fe.FontValue = orig;
                    if (fe.ShowDialog() == DialogResult.OK)
                    {
                        BPFont f = fe.FontValue;
                        if (f == null)
                        {
                            Store.DeleteFont(orig.Name);
                            gridFonts.Rows.Remove(row);
                        }
                        else
                        {
                            DataGridViewRow overwrittenRow = null;
                            if (orig.Name != f.Name)
                                overwrittenRow = FindFontRow(f.Name);
                            
                            // See if we're storing it with a new name and
                            // overwriting another font in the process
                            if (overwrittenRow != null)
                            {
                                DialogResult confirm = MessageBox.Show(null,
                                    Resources.ThisWillOverwriteAnExistingFontIsThatCorrect, Resources.AreYouSure,
                                    MessageBoxButtons.OKCancel,
                                    MessageBoxIcon.Question);
                                if (confirm != DialogResult.OK)
                                    return;
                            }

                            Store.SaveFont(f);

                            // Update the name, in case it was changed
                            row.Cells[0].Value = f.Name;

                            // And if we have an overwritten row, delete that.
                            if (overwrittenRow != null)
                                gridFonts.Rows.Remove(overwrittenRow);
                        }
                    }
                }
            }
            catch (NoSuchFontException)
            {
                MessageBox.Show(string.Format(
                    Resources.TheFont0DoesNotExistRefreshingFontsList));
                LoadFromStore();
            }
        }

        /// <summary>
        /// Handles the delete button being clicked.
        /// </summary>
        private void HandleDeleteClick(object sender, EventArgs e)
        {
            List<DataGridViewRow> toDelete = new List<DataGridViewRow>();
            foreach (DataGridViewRow row in gridFonts.SelectedRows)
            {
                if (UserMessage.OKCancel(string.Format(Resources.DeleteTheFont0, row.Cells[0].Value),
                    Resources.DeleteFont) == DialogResult.OK)
                {
                    Store.DeleteFont(row.Cells[0].Value as string);
                    toDelete.Add(row);
                }
            }

            foreach (DataGridViewRow row in toDelete)
                gridFonts.Rows.Remove(row);
        }

        /// <summary>
        /// Handles the font being double clicked - effectively treating it like
        /// an 'Edit' request.
        /// </summary>
        private void HandleFontDoubleClick(
            object sender, DataGridViewCellEventArgs e)
        {
            HandleEditClick(sender, e);
        }

        /// <summary>
        /// Handles the Spy operation being requested from the FontEditorForm
        /// created by this class to handle an Edit operation.
        /// </summary>
        private void HandleSpyRequested(object sender, SpyRequestEventArgs e)
        {
            OnSpyRequested(e);
        }

        private void HandleReferencesClick(object sender, EventArgs e)
        {
            ReferencesRequested(sender, e);
        }

        #endregion

    }
}
