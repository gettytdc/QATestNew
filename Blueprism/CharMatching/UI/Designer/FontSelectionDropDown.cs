using BluePrism.CharMatching.Properties;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using AutomateControls;
using AutomateControls.Forms;

namespace BluePrism.CharMatching.UI.Designer
{
    public partial class FontSelectionDropDown : UserControl
    {
        public event EventHandler FontSelectionAccepted;
        public event EventHandler FontSelectionCancelled;

        private SpyRegion _reg;
        private string _origSelected;
        private string _newFont;

        public FontSelectionDropDown(SpyRegion reg) : this(reg, "") { }

        public FontSelectionDropDown(SpyRegion reg, string selected)
        {
            InitializeComponent();
            _reg = reg;
            _origSelected = selected;

            foreach (string str in reg.Container.InstalledFontNames)
                lbFonts.Items.Add(str);

            SelectedFont = selected;
            // Add the listeners after the selection
            lbFonts.SelectedIndexChanged += HandleFontSelectionChanged;
            lbFonts.DoubleClick += HandleFontSelectionChanged;

            _worker.WorkerReportsProgress = true;
            _worker.WorkerSupportsCancellation = true;
            _worker.DoWork += HandleExtractFontWork;
            _worker.RunWorkerCompleted += HandleExtractFontDone;
        }

        public string SelectedFont
        {
            set { lbFonts.SelectedItem = value; }
            get { return (string)lbFonts.SelectedItem; }
        }

        protected virtual void OnFontSelectionAccepted(EventArgs e)
        {
            EventHandler handler = FontSelectionAccepted;
            if (handler != null)
                handler(this, e);
        }

        protected virtual void OnFontSelectionCancelled(EventArgs e)
        {
            EventHandler handler = FontSelectionCancelled;
            if (handler != null)
                handler(this, e);
        }

        private void HandleIdentifySystemFont(object sender, EventArgs e)
        {
            // If we can't go any further due to an invalid environment
            // then don't do so
            if (!FontConfig.ValidateEnvironment(TopLevelControl))
                return;

            using (FontIdentifierForm form = new FontIdentifierForm())
            {
                form.Size = new Size(800, 600);
                form.SpyRegionContainer = _reg.Container;

                // If they cancelled / closed - nothing to do
                if (form.ShowDialog() != DialogResult.OK)
                    return;

                // If they didn't select a font - nothing to do
                Font f = form.SelectedFont;
                if (f == null)
                    return;

                string fontLabel = string.Format("{0} {1} {2}", f.Name, f.Size,
                    f.Style==FontStyle.Regular ? "" : f.Style.ToString()).Trim();

                // Get a name for the font (can't be null or already used)
                string bpFontName = TextEnterBox.Show(this, Resources.GenerateABluePrismFont,
                    string.Format(Resources.PleaseEnterANameForTheBluePrismFontToBeGeneratedFromTheSystemFont0, fontLabel), fontLabel, false, true,
                    delegate(string text)
                    {
                        foreach (string font in lbFonts.Items)
                        {
                            if (text.Equals(font,
                                StringComparison.CurrentCultureIgnoreCase))
                            {
                                MessageBox.Show(String.Format(
                                    Resources.TheName0IsAlreadyInUsePleaseChooseAnother, text));
                                return false;
                            }
                        }
                        return true;
                    });

                // null => Cancelled - nothing to do
                if (bpFontName == null)
                    return;

                // Otherwise, generate a font.
                Dictionary<string,object> map = new Dictionary<string,object>();
                map["name"] = bpFontName;
                map["font"] = f;
                ProgressDialog.Show(
                    this.TopLevelControl, _worker, map, Resources.ExtractingFont, null);
            }
        }

        private void HandleExtractFontDone(object sender, RunWorkerCompletedEventArgs e)
        {
            Exception ex = e.Error;
            if (ex != null)
            {
                MessageBox.Show(ex.ToString(), Resources.Error);
                return;
            }
            if (e.Cancelled)
                return;
            BPFont f = e.Result as BPFont;
            if (f == null)
                return;
            _newFont = f.Name;
            int ind = lbFonts.Items.Add(_newFont);
            lbFonts.SelectedIndex = ind; // and that should see us off.
        }

        private void HandleExtractFontWork(object sender, DoWorkEventArgs e)
        {
            IDictionary<string, object> args =
                e.Argument as IDictionary<string, object>;

            string name = args["name"] as string;
            Font f = args["font"] as Font;
            IFontStore store = _reg.Container.Store;

            e.Result = SystemFontExtractor.ExtractAndSave(f, name, store,
                new BackgroundWorkerProgressMonitor(_worker), false);
        }

        private void HandleCreateEmptyFont(object sender, EventArgs e)
        {
            string name = (_newFont ?? ""); // default to empty or the last new font

            // Get a name for the font - don't allow empty and don't allow
            // any names which are already in use
            name = TextEnterBox.Show(this, Resources.FontName,
                Resources.PleaseEnterANameForTheFont, name, false, true,
                delegate(string text) {

                    foreach (string font in lbFonts.Items)
                    {
                        if (text.Equals(font,
                            StringComparison.CurrentCultureIgnoreCase))
                        {
                            MessageBox.Show(String.Format(
                                Resources.TheName0IsAlreadyInUsePleaseChooseAnother, text));
                            return false;
                        }
                    }
                    return true;
                }
            );

            if (name != null && name != _newFont)
            {
                // Remove the last created new font if there is one
                if (_newFont != null)
                    lbFonts.Items.Remove(_newFont);

                // Replace it with the new one.
                lbFonts.Items.Add(name);

                // And set it in case the user creates another 'new' one.
                _newFont = name;
            }
        }

        private void HandleOKClick(object sender, EventArgs e)
        {
            OnFontSelectionAccepted(EventArgs.Empty);
        }

        private void HandleFontSelectionChanged(object sender, EventArgs e)
        {
            if (lbFonts.SelectedItem != null)
                OnFontSelectionAccepted(EventArgs.Empty);
        }

        private void HandleClearClicked(object sender, EventArgs e)
        {
            lbFonts.SelectedItem = null;
            OnFontSelectionAccepted(EventArgs.Empty);
        }

    }
}
