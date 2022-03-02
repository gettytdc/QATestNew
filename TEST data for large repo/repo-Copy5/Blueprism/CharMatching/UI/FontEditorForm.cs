using BluePrism.CharMatching.Properties;
using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using AutomateControls;
using AutomateControls.Forms;

namespace BluePrism.CharMatching.UI
{
    /// <summary>
    /// Form containing a font editor and OK/Cancel buttons
    /// </summary>
    public partial class FontEditorForm : HelpButtonForm, IHelp
    {
        #region - Published Events -

        /// <summary>
        /// Event indicating that a spy operation has been requested by the font
        /// editor within this form.
        /// </summary>
        public event SpyRequestEventHandler SpyRequested;

        #endregion

        #region - Constructors -

        /// <summary>
        /// Creates a new font editor form
        /// </summary>
        public FontEditorForm()
        {
            InitializeComponent();
            btnOk.Enabled = false;
        }

        #endregion

        #region - Properties -

        /// <summary>
        /// The font value being edited by this control
        /// </summary>
        [Browsable(false),
         DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public BPFont FontValue
        {
            get { return editor.FontValue; }
            set
            {
                editor.FontValue = value;
                btnOk.Enabled = (value != null);
                Text = Resources.FontEditor + (value == null ? "" : " : " + value.Name);
            }
        }

        /// <summary>
        /// The image displayed in the font editor in this form
        /// </summary>
        public Image Image
        {
            get { return editor.Image; }
            set { editor.Image = value; }
        }

        /// <summary>
        /// Indicates if the region editor on this form is visible or not
        /// </summary>
        public bool RegionEditorVisible
        {
            get { return editor.RegionEditorVisible; }
            set { editor.RegionEditorVisible = value; }
        }

        /// <summary>
        /// The font store from which fonts can be loaded and to which fonts can
        /// be saved
        /// </summary>
        [Browsable(false),
         DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public IFontStore Store
        {
            get { return editor.Store; }
            set { editor.Store = value; }
        }

        #endregion

        #region - Methods -

        /// <summary>
        /// Handles the OK button being clicked on the form
        /// </summary>
        private void HandleOkClick(object sender, EventArgs e)
        {
            DialogResult = DialogResult.OK;
            Close();
        }

        /// <summary>
        /// Handles the cancel button being clicked on the form
        /// </summary>
        private void HandleCancelClick(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }

        /// <summary>
        /// Raises the <see cref="SpyRequested"/> event.
        /// </summary>
        protected virtual void OnSpyRequested(SpyRequestEventArgs e)
        {
            SpyRequestEventHandler handler = this.SpyRequested;
            if (handler != null)
                handler(this, e);
        }

        /// <summary>
        /// Handles the SpyRequested event from the FontEditor, propogating it to
        /// any parties interested in the event listening to this form.
        /// </summary>
        private void HandleSpyRequested(object sender, SpyRequestEventArgs e)
        {
            OnSpyRequested(e);
        }

        #endregion


        string IHelp.GetHelpFile()
        {
            return "helpFonts.htm";
        }
    }
}
