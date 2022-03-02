using System;
using System.ComponentModel;
using System.Windows.Forms;

namespace BluePrism.CharMatching.UI.Designer
{
    public partial class GridSpyRegionSchemaEditorForm : Form
    {
        public GridSpyRegionSchemaEditorForm()
        {
            InitializeComponent();
        }

        public GridSpyRegionSchema Schema
        {
            get { return editor.Schema; }
            set { editor.Schema = value; }
        }

        private void HandleOk(object sender, EventArgs e)
        {
            DialogResult = DialogResult.OK;
            Close();
        }

        private void HandleCancel(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            if (DialogResult == DialogResult.None)
                DialogResult = DialogResult.Cancel;
            base.OnClosing(e);
        }
    }
}
