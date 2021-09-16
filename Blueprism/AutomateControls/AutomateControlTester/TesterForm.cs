using System;
using System.Collections.Generic;
using System.Windows.Forms;
using AutomateControlTester.Configurators;
using System.Reflection;

namespace AutomateControlTester
{
    /// <summary>
    /// The idea is to load up all the controls found in AutomateControls and
    /// let the user choose one - then create an instance of that control, and
    /// display it, Docked to Fill inside a form for basic testing.
    /// Any further customisation can be done in the control itself for the
    /// purposes of the test. Obviously not yet complete.
    /// </summary>
    public partial class TesterForm : Form
    {
        public TesterForm()
        {
            InitializeComponent();
            gridControls.SelectionChanged += new EventHandler(HandleSelectChanged);
        }

        DataGridViewRow SelectedRow
        {
            get
            {
                int count = gridControls.SelectedRows.Count;
                if (count == 0)
                    return null;
                if (count == 1)
                    return gridControls.SelectedRows[0];
                return null;
            }
        }

        void HandleSelectChanged(object sender, EventArgs e)
        {
            DataGridViewRow row = SelectedRow;

            IConfigurator cfg = null;
            if (row != null)
                cfg = row.Tag as IConfigurator;

            Control ctl = null;
            if (cfg != null)
                ctl = cfg.Control;

            if (gpConfig.Controls.Count > 0 && gpConfig.Controls[0] == ctl)
                return;

            gpConfig.Controls.Clear();
            if (ctl == null)
                return;

            ctl.Dock = DockStyle.Fill;
            gpConfig.Controls.Add(ctl);
        }

        public ICollection<IConfigurator> Configurators
        {
            get
            {
                List<IConfigurator> configs = new List<IConfigurator>();
                foreach (DataGridViewRow row in gridControls.Rows)
                {
                    configs.Add((IConfigurator)row.Tag);
                }
                return configs;
            }
            set
            {
                gridControls.Rows.Clear();
                gpConfig.Controls.Clear();
                if (value == null)
                    return;
                foreach (IConfigurator cfg in value)
                {
                    DataGridViewRow row = new DataGridViewRow();
                    row.Tag = cfg;
                    DataGridViewCell cell = new DataGridViewTextBoxCell();
                    cell.Value = cfg.ConfigName;
                    row.Cells.Add(cell);
                    gridControls.Rows.Add(row);
                }
            }
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            List<IConfigurator> configs = new List<IConfigurator>();
            foreach (Type t in Assembly.GetExecutingAssembly().GetTypes())
            {
                if (t != typeof(BaseConfigurator) && t.IsClass && !t.IsAbstract &&
                    typeof(IConfigurator).IsAssignableFrom(t))
                {
                    configs.Add((IConfigurator)
                        t.GetConstructor(Type.EmptyTypes).Invoke(null));
                }
            }
            // Sort by config name
            configs.Sort((x, y) => x.ConfigName.CompareTo(y.ConfigName));
            this.Configurators = configs;
        }


        private void HandleControlActivated(object sender, EventArgs e)
        {
        }

        private void btnOK_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            Close();
        }
    }
}