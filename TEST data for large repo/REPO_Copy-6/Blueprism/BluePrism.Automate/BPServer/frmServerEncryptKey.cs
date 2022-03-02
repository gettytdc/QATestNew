using BluePrism.BPServer.Properties;
using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using AutomateControls;
using BluePrism.AutomateAppCore;
using BluePrism.BPCoreLib;
using BluePrism.Common.Security;

namespace BluePrism.BPServer
{
    public partial class frmServerEncryptKey : Form
    {
        private int mLastSelectedDefaultAlgorithm;
        public clsEncryptionScheme Encrypter
        {
            get { return _encrypter; }
            set
            {
                _encrypter = value;
                PopulateUI();
            }
        }
        private clsEncryptionScheme _encrypter;

        public List<string> UsedNames
        {
            get { return _usedNames; }
            set { _usedNames = value; }
        }
        private List<string> _usedNames = new List<string>();

        public frmServerEncryptKey()
        {
            InitializeComponent();

            foreach (EncryptionAlgorithm alg in clsEncryptionScheme.GetOrderedAlgorithms())
            {
                var name = alg.GetFriendlyName(true);
                var enabled = true;

                if (!clsFIPSCompliance.CheckForFIPSCompliance(alg))
                {
                    name += Resources.NotFIPSCompliant;
                    enabled = false;
                }
                var entry = new ComboBoxItem(name, alg, enabled)
                {
                    Selectable = enabled
                };

                cmbAlgorithm.Items.Add(entry);
            }

            mLastSelectedDefaultAlgorithm = cmbAlgorithm.SelectedIndex = 0;

            //Default to hide keys
            txtKey.UseSystemPasswordChar = true;
        }

        private void PopulateUI()
        {
            txtName.Text = _encrypter.Name;

            // Crossing the boundary here to where revealing the encryption key
            // is a "UI feature" (no other way to put one in or get one out).
            var plainKey = new StringBuilder();
            using (var pinned = _encrypter.Key.Pin())
                foreach (char c in pinned.Chars)
                    plainKey.Append(c);
            txtKey.Text = plainKey.ToString();

            cmbAlgorithm.SelectedIndex =
                cmbAlgorithm.FindString(_encrypter.AlgorithmName);
        }

        /// <summary>
        /// Handler to cope with the Generate Key button being clicked.
        /// </summary>
        private void HandleGenerateKey(object sender, LinkLabelLinkClickedEventArgs e)
        {
            clsEncryptionScheme scheme = new clsEncryptionScheme(txtName.Text);
            ComboBoxItem item = (ComboBoxItem)cmbAlgorithm.SelectedItem;
            scheme.Algorithm = (EncryptionAlgorithm)item.Tag;
            scheme.GenerateKey();

            // Crossing the boundary here to where revealing the encryption key
            // is a "UI feature" (no other way to put one in or get one out).
            var plainKey = new StringBuilder();
            using (var pinned = scheme.Key.Pin())
                foreach (char c in pinned.Chars)
                    plainKey.Append(c);
            txtKey.Text = plainKey.ToString();
        }

        /// <summary>
        /// Handles the 'show key' checkbox being checked or unchecked
        /// </summary>
        private void HandleToggleKey(object sender, EventArgs e)
        {
            txtKey.UseSystemPasswordChar = !chkShowKey.Checked;
        }

        /// <summary>
        /// Handles the OK button being pressed
        /// </summary>
        private void HandleOK(object sender, EventArgs e)
        {
            _encrypter = new clsEncryptionScheme(txtName.Text);
            ComboBoxItem item = (ComboBoxItem)cmbAlgorithm.SelectedItem;
            _encrypter.Algorithm = (EncryptionAlgorithm)item.Tag;
            _encrypter.Key = new SafeString(txtKey.Text);

            if (_encrypter.Name == string.Empty)
            {
                MessageBox.Show(this, Resources.PleaseEnterTheEncryptionSchemeNameForThisKey,
                    Resources.EncryptionKeyError, MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            else if (_usedNames.Contains(_encrypter.Name))
            {
                MessageBox.Show(this, Resources.AKeyHasAlreadyBeenAssignedToEncryptionScheme + _encrypter.Name,
                    Resources.EncryptionKeyError, MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            else if (!_encrypter.HasValidKey)
            {
                MessageBox.Show(this, Resources.KeyNotValidForSelectedEncryptionMethod,
                    Resources.EncryptionKeyError, MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            DialogResult = DialogResult.OK;
            Close();
        }

        /// <summary>
        /// Handles the Cancel button being pressed
        /// </summary>
        private void HandleCancel(object sender, EventArgs e)
        {
            _encrypter = null;
            DialogResult = DialogResult.Cancel;
            Close();
        }

        /// <summary>
        /// Show a warning if a retired scheme is selected.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void cmbAlgorithm_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (cmbAlgorithm.SelectedItem != null)
            {
                ComboBoxItem item = (ComboBoxItem)cmbAlgorithm.SelectedItem;
                if (item != null && item.Tag != null)
                {
                    var alg = (EncryptionAlgorithm)item.Tag;
                    lblRetiredWarning.Visible = alg.IsRetired();
                }

                mLastSelectedDefaultAlgorithm = cmbAlgorithm.SelectedIndex;
            }
        }

        private void cmbAlgorithm_LostFocus(object sender, EventArgs e)
        {
            if (cmbAlgorithm.SelectedItem.ToString().Contains(Resources.NotFIPSCompliant))
            {
                cmbAlgorithm.SelectedIndex = mLastSelectedDefaultAlgorithm;
            }
        }
    }
}
