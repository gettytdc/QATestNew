using System.Windows.Forms;

namespace AutomateControls.DataGridViews
{
    public partial class StyledDataGridViewButtonColumn : DataGridViewButtonColumn
    {
        public StyledDataGridViewButtonColumn()
        {
            CellTemplate = new StyledDataGridViewButtonCell();
        }

        public sealed override DataGridViewCell CellTemplate
        {
            get { return base.CellTemplate; }
            set { base.CellTemplate = value; }
        }
    }
}
