using System.Drawing;
using System.Windows.Forms;
using AutomateControls;

namespace AutomateControlTester.Configurators
{
    public partial class ComboBoxConfigurator : BaseConfigurator
    {
        public ComboBoxConfigurator()
        {
            InitializeComponent();
            AddTo(cmbStyle);
            AddTo(cmbNormal);
        }

        private void AddTo(ComboBox combo)
        {
            combo.Items.AddRange(new object[]{
                new ComboBoxItem("None"),
                new ComboBoxItem("One", 1),
                new ComboBoxItem("Too", 2, false),
                new ComboBoxItem("Two", 2),
                new ComboBoxItem("Three", Color.Red, 3)
            });
        }

        public override string ConfigName { get { return "Combo Box"; } }
    }
}
