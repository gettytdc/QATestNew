using System;
using AutomateControls.Forms;

namespace BluePrism.CharMatching.UI
{
    /// <summary>
    /// Form to hold a <see cref="FontGenerator"/>, able to generate Blue Prism fonts
    /// from installed system fonts.
    /// </summary>
    public partial class FontGeneratorForm : AutomateForm
    {
        /// <summary>
        /// Creates a new initialised font generator form and loads the system fonts
        /// into the embedded font generator control
        /// </summary>
        public FontGeneratorForm()
        {
            InitializeComponent();
            generator.LoadFonts();
        }

        /// <summary>
        /// The store which is used to get and set BP font data.
        /// </summary>
        public IFontStore Store
        {
            get { return generator.Store; }
            set { generator.Store = value; }
        }

        /// <summary>
        /// Handles the close button being pressed
        /// </summary>
        private void HandleCloseClicked(object sender, EventArgs e)
        {
            Close();
        }
    }
}
