using BluePrism.CharMatching.Properties;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace BluePrism.CharMatching.UI
{
    /// <summary>
    /// Form to display the search parameters for the scan of system fonts
    /// </summary>
    public partial class FontScannerSearchParamsForm : Form
    {
        #region - Member Variables -

        // Map of checkboxes onto font styles
        private IDictionary<FontStyle, CheckBox> _styleBoxes;

        // Map of checkboxes onto font sizes
        private IDictionary<float, CheckBox> _sizeBoxes;

        // Map of checkboxes onto render methods
        private IDictionary<RenderMethod, CheckBox> _renderBoxes;

        #endregion

        #region - Constructors -

        /// <summary>
        /// Creates a new initialized font scanner params form
        /// </summary>
        public FontScannerSearchParamsForm()
        {
            InitializeComponent();

            // Maps provide a mechanism to get from value => UI Element
            _styleBoxes = new Dictionary<FontStyle, CheckBox>();
            _sizeBoxes = new Dictionary<float, CheckBox>();
            _renderBoxes = new Dictionary<RenderMethod, CheckBox>();
            
            // Add a look up so we can get from the value to the checkbox easily
            _styleBoxes[FontStyle.Regular] = cbRegular;
            _styleBoxes[FontStyle.Bold] = cbBold;
            _styleBoxes[FontStyle.Italic] = cbItalic;
            _styleBoxes[FontStyle.Strikeout] = cbStrike;
            _styleBoxes[FontStyle.Underline] = cbUnderline;

            _sizeBoxes[6f] = cb6;
            _sizeBoxes[7f] = cb7;
            _sizeBoxes[7.5f] = cb7_5;
            _sizeBoxes[8f] = cb8;
            _sizeBoxes[8.5f] = cb8_5;
            _sizeBoxes[9f] = cb9;
            _sizeBoxes[9.5f] = cb9_5;
            _sizeBoxes[10f] = cb10;
            _sizeBoxes[11f] = cb11;
            _sizeBoxes[12f] = cb12;
            _sizeBoxes[13f] = cb13;
            _sizeBoxes[14f] = cb14;
            _sizeBoxes[15f] = cb15;
            _sizeBoxes[16f] = cb16;
            _sizeBoxes[18f] = cb18;
            _sizeBoxes[20f] = cb20;
            _sizeBoxes[22f] = cb22;
            _sizeBoxes[24f] = cb24;
            _sizeBoxes[28f] = cb28;
            _sizeBoxes[32f] = cb32;
            _sizeBoxes[36f] = cb36;

            _renderBoxes[RenderMethod.GDI] = cbGDI;
            _renderBoxes[RenderMethod.GDIPlus] = cbGDIPlus;

            // And implement a reverse-lookup by setting the values as tags in
            // their corresponding checkboxes
            foreach (KeyValuePair<FontStyle, CheckBox> pair in _styleBoxes)
            {
                pair.Value.Tag = pair.Key;
            }
            foreach (KeyValuePair<float, CheckBox> pair in _sizeBoxes)
            {
                pair.Value.Tag = pair.Key;
            }
            foreach (KeyValuePair<RenderMethod, CheckBox> pair in _renderBoxes)
            {
                pair.Value.Tag = pair.Key;
            }
        }

        #endregion

        #region - Properties -

        /// <summary>
        /// Gets or sets the settings represented on this form
        /// </summary>
        public FontSearchDetailSettings Settings
        {
            get
            {
                FontSearchDetailSettings s = new FontSearchDetailSettings();
                foreach (CheckBox cb in flowStyles.Controls)
                {
                    if (cb.Checked)
                        s.Styles.Add((FontStyle)cb.Tag);
                }
                foreach (CheckBox cb in flowSizes.Controls)
                {
                    if (cb.Checked)
                        s.Ems.Add((float)cb.Tag);
                }
                foreach (CheckBox cb in flowRenders.Controls)
                {
                    if (cb.Checked)
                        s.RenderMethods.Add((RenderMethod)cb.Tag);
                }
                return s;
            }
            set
            {
                Clear();
                foreach (FontStyle style in value.Styles)
                {
                    _styleBoxes[style].Checked = true;
                }
                foreach (float em in value.Ems)
                {
                    _sizeBoxes[em].Checked = true;
                }
                foreach (RenderMethod meth in value.RenderMethods)
                {
                    _renderBoxes[meth].Checked = true;
                }
            }
        }

        #endregion

        #region - Methods -

        /// <summary>
        /// Clears all the search parameter values on this form (ie. unchecks all
        /// checkboxes)
        /// </summary>
        public void Clear()
        {
            foreach (CheckBox cb in _styleBoxes.Values)
                cb.Checked = false;
            foreach (CheckBox cb in _sizeBoxes.Values)
                cb.Checked = false;
            foreach (CheckBox cb in _renderBoxes.Values)
                cb.Checked = false;
        }

        /// <summary>
        /// Handles the OK button being clicked
        /// </summary>
        private void HandleOkClick(object sender, EventArgs e)
        {
            if (!Settings.IsValid)
            {
                MessageBox.Show(Resources.YouMustTickAtLeastOneCheckboxInEachGroup,
                    Resources.Error, MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            DialogResult = DialogResult.OK;
            Close();
        }

        /// <summary>
        /// Handles the Cancel button being clicked
        /// </summary>
        private void HandleCancelClick(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }

        #endregion

    }
}
